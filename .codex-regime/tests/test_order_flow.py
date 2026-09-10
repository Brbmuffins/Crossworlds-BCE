from decimal import Decimal
import unittest
from rh_crypto_bot.order_flow import MinuteFlow

class OrderFlowTests(unittest.TestCase):
    def test_aggregates_depth_imbalance_and_trades(self):
        flow=MinuteFlow(["BTC-USD"])
        flow.update_book("BTC-USD", [{"side":"bid","price_level":"99","new_quantity":"2"},{"side":"offer","price_level":"101","new_quantity":"1"}])
        flow.update_trades("BTC-USD", [{"side":"BUY","price":"100","size":".5"}])
        row=flow.rows(60)[0]
        self.assertEqual(row[1],"BTC-USD"); self.assertEqual(Decimal(row[7]),Decimal("97")/Decimal("299")); self.assertEqual(row[8],"50.0")
