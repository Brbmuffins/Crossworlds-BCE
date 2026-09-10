from decimal import Decimal
import unittest

from rh_crypto_bot.market_data import Candle
from rh_crypto_bot.regime import classify_regime, market_leadership


def candles(count: int, step: Decimal) -> list[Candle]:
    rows = []
    for index in range(count):
        price = Decimal("100") + step * index
        rows.append(Candle(index * 14400, price - 1, price + 1, price, price, Decimal("10")))
    return rows


class RegimeTests(unittest.TestCase):
    def test_steady_uptrend_permits_long_breakout_context(self) -> None:
        result = classify_regime(candles(360, Decimal("0.2")))
        self.assertEqual(result.trend, "bullish")
        self.assertTrue(result.daily_confirmation)
        self.assertTrue(result.permits_long_breakout)

    def test_downtrend_is_vetoed(self) -> None:
        result = classify_regime(candles(360, Decimal("-0.1")))
        self.assertEqual(result.trend, "bearish")
        self.assertFalse(result.permits_long_breakout)

    def test_discontinuous_history_is_rejected(self) -> None:
        rows = candles(360, Decimal("0.1"))
        rows[-1] = Candle(rows[-1].start + 14400, rows[-1].low, rows[-1].high,
                          rows[-1].open, rows[-1].close, rows[-1].volume)
        with self.assertRaises(ValueError):
            classify_regime(rows)

    def test_market_leadership_detects_broad_altcoin_move(self) -> None:
        btc = candles(360, Decimal("0.02"))
        alt = candles(360, Decimal("0.2"))
        self.assertEqual(
            market_leadership(btc, {"BTC-USD": btc, "ALT-USD": alt}, Decimal("0.8")),
            "altcoin_breadth",
        )
