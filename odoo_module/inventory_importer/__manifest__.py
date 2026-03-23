{
    'name': 'Inventory Importer',
    'version': '16.0.1.0.0',
    'summary': 'Import inventory data from InventoryApp via REST API',
    'description': """
        Allows importing inventory data (title, fields, aggregated statistics)
        from the InventoryApp course project via its REST API using an API token.
        Supports view of imported inventories with detailed field statistics.
    """,
    'category': 'Inventory',
    'author': 'Course Project',
    'depends': ['base', 'web'],
    'data': [
        'security/ir.model.access.csv',
        'views/inventory_import_views.xml',
        'views/menu.xml',
    ],
    'installable': True,
    'application': True,
    'license': 'LGPL-3',
}
