from __future__ import annotations

from dataclasses import dataclass
from decimal import Decimal

from .market_data import Candle
from .strategy import Signal


@dataclass(frozen=True)
class EventStrategyConfig:
    breakout_window: int = 30
    atr_window: int = 20
    atr_multiplier: Decimal = Decimal("1.50")
    volume_window: int = 30
    volume_multiplier: Decimal = Decimal("1.50")
    trend_window: int = 120
    relative_strength_window: int = 30
    minimum_relative_strength: Decimal = Decimal("0.03")
    breadth_window: int = 50
    minimum_breadth: Decimal = Decimal("0.60")
    expected_interval_seconds: int = 14_400

    @property
    def warmup(self) -> int:
        return max(
            self.breakout_window,
            self.atr_window + 1,
            self.volume_window,
            self.trend_window + 1,
            self.relative_strength_window + 1,
            self.breadth_window,
        )

    def validate(self) -> None:
        windows = (
            self.breakout_window,
            self.atr_window,
            self.volume_window,
            self.trend_window,
            self.relative_strength_window,
            self.breadth_window,
            self.expected_interval_seconds,
        )
        if min(windows) < 1:
            raise ValueError("Event strategy windows must be positive")
        if min(self.atr_multiplier, self.volume_multiplier) <= 0:
            raise ValueError("Expansion multipliers must be positive")
        if not 0 <= self.minimum_breadth <= 1:
            raise ValueError("minimum_breadth must be between 0 and 1")


def event_strategy_profile(
    profile: str, *, expected_interval_seconds: int = 14_400
) -> EventStrategyConfig:
    """Return a frozen shadow-research profile; risk vetoes live outside this config."""
    if profile == "strict":
        return EventStrategyConfig(expected_interval_seconds=expected_interval_seconds)
    if profile == "balanced":
        return EventStrategyConfig(
            breakout_window=20,
            atr_multiplier=Decimal("1.25"),
            volume_multiplier=Decimal("1.25"),
            trend_window=90,
            minimum_relative_strength=Decimal("0.015"),
            minimum_breadth=Decimal("0.50"),
            expected_interval_seconds=expected_interval_seconds,
        )
    raise ValueError(f"Unknown event strategy profile: {profile}")


def _mean(values: list[Decimal]) -> Decimal:
    return sum(values, Decimal("0")) / Decimal(len(values))


def _continuous(candles: list[Candle], start: int, end: int, seconds: int) -> bool:
    return all(
        right.start - left.start == seconds
        for left, right in zip(candles[start:end], candles[start + 1 : end + 1])
    )


def _true_range(candle: Candle, previous_close: Decimal) -> Decimal:
    return max(
        candle.high - candle.low,
        abs(candle.high - previous_close),
        abs(candle.low - previous_close),
    )


def market_breadth(
    universe: dict[str, list[Candle]], config: EventStrategyConfig
) -> dict[int, Decimal]:
    """Fraction of available assets above their trailing breadth moving average."""
    votes: dict[int, list[bool]] = {}
    for candles in universe.values():
        for index in range(config.breadth_window, len(candles)):
            if not _continuous(
                candles,
                index - config.breadth_window,
                index,
                config.expected_interval_seconds,
            ):
                continue
            average = _mean(
                [c.close for c in candles[index - config.breadth_window : index]]
            )
            votes.setdefault(candles[index].start, []).append(candles[index].close > average)
    return {
        timestamp: Decimal(sum(values)) / Decimal(len(values))
        for timestamp, values in votes.items()
        if values
    }


def high_conviction_event_signals(
    candles: list[Candle],
    benchmark: list[Candle],
    breadth: dict[int, Decimal],
    config: EventStrategyConfig = EventStrategyConfig(),
    *,
    is_benchmark: bool = False,
) -> list[Signal]:
    config.validate()
    benchmark_index = {candle.start: index for index, candle in enumerate(benchmark)}
    signals: list[Signal] = []
    for index in range(config.warmup, len(candles)):
        timestamp = candles[index].start
        btc_index = benchmark_index.get(timestamp)
        if btc_index is None or btc_index < config.warmup:
            continue
        if not _continuous(
            candles, index - config.warmup, index, config.expected_interval_seconds
        ) or not _continuous(
            benchmark,
            btc_index - config.warmup,
            btc_index,
            config.expected_interval_seconds,
        ):
            continue
        if breadth.get(timestamp, Decimal("0")) < config.minimum_breadth:
            continue
        asset_trend = _mean(
            [c.close for c in candles[index - config.trend_window : index]]
        )
        btc_trend = _mean(
            [c.close for c in benchmark[btc_index - config.trend_window : btc_index]]
        )
        if candles[index].close <= asset_trend or benchmark[btc_index].close <= btc_trend:
            continue
        prior_high = max(
            c.high for c in candles[index - config.breakout_window : index]
        )
        if candles[index].close <= prior_high:
            continue
        prior_ranges = [
            _true_range(candles[position], candles[position - 1].close)
            for position in range(index - config.atr_window, index)
        ]
        current_range = _true_range(candles[index], candles[index - 1].close)
        if current_range < _mean(prior_ranges) * config.atr_multiplier:
            continue
        average_volume = _mean(
            [c.volume for c in candles[index - config.volume_window : index]]
        )
        if candles[index].volume < average_volume * config.volume_multiplier:
            continue
        asset_return = (
            candles[index].close
            / candles[index - config.relative_strength_window].close
            - 1
        )
        btc_return = (
            benchmark[btc_index].close
            / benchmark[btc_index - config.relative_strength_window].close
            - 1
        )
        relative_strength = asset_return - btc_return
        if not is_benchmark and relative_strength < config.minimum_relative_strength:
            continue
        signals.append(
            Signal(
                index,
                timestamp,
                (
                    f"high_conviction_event; breadth={breadth[timestamp]:.2%}; "
                    f"range_expansion={current_range / _mean(prior_ranges):.2f}x; "
                    f"relative_strength={relative_strength:.2%}"
                ),
            )
        )
    return signals
