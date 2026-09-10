from __future__ import annotations

from dataclasses import dataclass
from decimal import Decimal

from .market_data import Candle


def _mean(values: list[Decimal]) -> Decimal:
    return sum(values, Decimal("0")) / Decimal(len(values))


def _true_range(candle: Candle, previous: Decimal) -> Decimal:
    return max(candle.high - candle.low, abs(candle.high - previous), abs(candle.low - previous))


@dataclass(frozen=True)
class Regime:
    trend: str
    volatility: str
    phase: str
    leadership: str
    daily_confirmation: bool
    permits_long_breakout: bool
    reasons: tuple[str, ...]
    trend_distance: Decimal
    atr_rate: Decimal

    def as_json(self) -> dict[str, object]:
        return {
            "trend": self.trend,
            "volatility": self.volatility,
            "phase": self.phase,
            "leadership": self.leadership,
            "daily_confirmation": self.daily_confirmation,
            "permits_long_breakout": self.permits_long_breakout,
            "reasons": list(self.reasons),
            "trend_distance": str(self.trend_distance.quantize(Decimal("0.0001"))),
            "atr_rate": str(self.atr_rate.quantize(Decimal("0.0001"))),
        }


def _period_return(candles: list[Candle], bars: int) -> Decimal | None:
    if len(candles) <= bars:
        return None
    return candles[-1].close / candles[-1 - bars].close - 1


def market_leadership(
    benchmark: list[Candle], universe: dict[str, list[Candle]], breadth: Decimal, bars: int = 30
) -> str:
    btc_return = _period_return(benchmark, bars)
    alt_returns = [
        value for symbol, candles in universe.items()
        if symbol != "BTC-USD" and (value := _period_return(candles, bars)) is not None
    ]
    if btc_return is None or not alt_returns:
        return "unknown"
    alt_return = _mean(alt_returns)
    if btc_return < 0 and breadth < Decimal("0.40"):
        return "risk_off"
    if breadth >= Decimal("0.60") and alt_return > btc_return + Decimal("0.02"):
        return "altcoin_breadth"
    if btc_return > alt_return + Decimal("0.02"):
        return "bitcoin_led"
    return "mixed"


def classify_regime(
    candles: list[Candle], *, leadership: str = "mixed", expected_interval: int = 14_400
) -> Regime:
    if len(candles) < 300:
        raise ValueError("At least 300 four-hour candles are required for regime classification")
    recent = candles[-300:]
    if any(right.start - left.start != expected_interval for left, right in zip(recent, recent[1:])):
        raise ValueError("Recent candles are discontinuous")
    close = candles[-1].close
    fast = _mean([c.close for c in candles[-30:]])
    slow = _mean([c.close for c in candles[-120:]])
    previous_slow = _mean([c.close for c in candles[-132:-12]])
    slow_slope = slow / previous_slow - 1
    if close > slow and fast > slow and slow_slope > 0:
        trend = "bullish"
    elif close < slow and fast < slow and slow_slope < 0:
        trend = "bearish"
    else:
        trend = "sideways"

    ranges = [
        _true_range(candles[index], candles[index - 1].close)
        for index in range(len(candles) - 20, len(candles))
    ]
    earlier_ranges = [
        _true_range(candles[index], candles[index - 1].close)
        for index in range(len(candles) - 40, len(candles) - 20)
    ]
    atr_rate = _mean(ranges) / close
    volatility = (
        "high" if atr_rate > Decimal("0.06") or _mean(ranges) > _mean(earlier_ranges) * Decimal("1.75")
        else "normal"
    )
    distance = (close - slow) / slow
    recent_return = close / candles[-13].close - 1
    prior_fast = _mean([c.close for c in candles[-42:-12]])
    if trend == "bullish" and prior_fast <= previous_slow:
        phase = "early"
    elif trend == "bullish" and recent_return < 0 and close > slow:
        phase = "pullback"
    elif trend == "bullish" and distance > atr_rate * Decimal("8"):
        phase = "exhausted"
    elif trend == "bullish":
        phase = "established"
    elif trend == "bearish" and recent_return > 0:
        phase = "reversal_risk"
    else:
        phase = "declining" if trend == "bearish" else "consolidating"

    daily_closes = [candles[index].close for index in range(len(candles) - 1, -1, -6)][::-1]
    daily_confirmation = (
        len(daily_closes) >= 50
        and daily_closes[-1] > _mean(daily_closes[-50:])
        and _mean(daily_closes[-20:]) > _mean(daily_closes[-50:])
    )
    reasons = []
    if trend != "bullish":
        reasons.append(f"trend={trend}")
    if phase not in {"early", "established"}:
        reasons.append(f"phase={phase}")
    if volatility == "high":
        reasons.append("high_volatility")
    if not daily_confirmation:
        reasons.append("daily_trend_unconfirmed")
    if leadership == "risk_off":
        reasons.append("market_risk_off")
    return Regime(
        trend, volatility, phase, leadership, daily_confirmation, not reasons,
        tuple(reasons), distance, atr_rate,
    )
