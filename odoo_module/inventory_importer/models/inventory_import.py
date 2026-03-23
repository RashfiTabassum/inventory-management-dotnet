import requests
import json
from odoo import models, fields, api
from odoo.exceptions import UserError


class ImportedInventory(models.Model):
    _name = 'inventory.imported'
    _description = 'Imported Inventory'
    _order = 'import_date desc'

    name = fields.Char(string='Inventory Title', required=True, readonly=True)
    description = fields.Text(string='Description', readonly=True)
    category = fields.Char(string='Category', readonly=True)
    item_count = fields.Integer(string='Item Count', readonly=True)
    api_token = fields.Char(string='API Token', required=True)
    api_base_url = fields.Char(
        string='API Base URL',
        required=True,
        default='https://your-app.onrender.com',
        help='Base URL of the InventoryApp instance (no trailing slash)',
    )
    import_date = fields.Datetime(string='Last Imported', readonly=True)
    field_ids = fields.One2many(
        'inventory.imported.field', 'inventory_id', string='Fields', readonly=True
    )

    def action_import(self):
        """Fetch data from InventoryApp REST API and store it."""
        for record in self:
            url = f"{record.api_base_url.rstrip('/')}/api/inventory/{record.api_token}"
            try:
                response = requests.get(url, timeout=15)
            except requests.exceptions.RequestException as e:
                raise UserError(f"Connection error: {e}")

            if response.status_code == 404:
                raise UserError("Invalid API token or inventory not found.")
            if response.status_code != 200:
                raise UserError(f"API returned HTTP {response.status_code}.")

            data = response.json()

            record.write({
                'name': data.get('inventoryTitle', ''),
                'description': data.get('description', ''),
                'category': data.get('category', ''),
                'item_count': data.get('itemCount', 0),
                'import_date': fields.Datetime.now(),
            })

            # Remove old fields
            record.field_ids.unlink()

            # Create new field records
            for f in data.get('fields', []):
                agg = f.get('aggregation', {})
                top_values_raw = agg.get('topValues', [])
                top_values_str = ', '.join(
                    f"{tv['value']} ({tv['count']})" for tv in top_values_raw
                ) if top_values_raw else ''

                self.env['inventory.imported.field'].create({
                    'inventory_id': record.id,
                    'name': f.get('title', ''),
                    'field_type': f.get('type', ''),
                    'agg_average': agg.get('average'),
                    'agg_min': agg.get('min'),
                    'agg_max': agg.get('max'),
                    'agg_top_values': top_values_str,
                })

        return {
            'type': 'ir.actions.client',
            'tag': 'display_notification',
            'params': {
                'title': 'Import successful',
                'message': f"Inventory '{self.name}' imported with {self.item_count} items.",
                'type': 'success',
                'sticky': False,
            },
        }


class ImportedInventoryField(models.Model):
    _name = 'inventory.imported.field'
    _description = 'Imported Inventory Field'
    _order = 'name'

    inventory_id = fields.Many2one(
        'inventory.imported', string='Inventory', ondelete='cascade', required=True
    )
    name = fields.Char(string='Field Title', required=True)
    field_type = fields.Char(string='Type')

    # Numeric aggregation
    agg_average = fields.Float(string='Average', digits=(16, 2))
    agg_min = fields.Float(string='Min')
    agg_max = fields.Float(string='Max')

    # Text aggregation
    agg_top_values = fields.Text(string='Top Values')

    @property
    def is_numeric(self):
        return self.field_type in ('Numeric',)
