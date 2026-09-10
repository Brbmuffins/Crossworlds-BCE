from __future__ import annotations

import argparse
from datetime import datetime, timedelta, timezone
from decimal import Decimal
import json

from .client import RobinhoodReadOnlyClient
from .backtest import BacktestConfig, result_summary, run_backtest
from .config import Settings
from .costs import RoundTripCosts, net_return, required_exit_price
from .market_data import CoinbasePublicMarketData, GRANULARITY_SECONDS
from .storage import CandleStore
from .validation import validate_candles
from .strategy import StrategyConfig, trend_momentum_signals
from .universe import select_candidates
from .evaluation import walk_forward, walk_forward_summary
from .breakout_evaluation import breakout_summary, evaluate_breakout_with_holdout
from .matrix import evaluate_target_horizon_matrix, matrix_summary
from .event_evaluation import event_summary, evaluate_event_folds
from .event_strategy import EventStrategyConfig, event_strategy_profile
from .event_strategy import high_conviction_event_signals, market_breadth
from .shadow import ShadowStore, execution_price, quantity_for_notional, shadow_levels
from .execution_quality import (
    batched_execution_prices, execution_quality, return_correlation, volatility_target,
)
from .discord import (
    DiscordWebhook, action_message, promotion_ready_message, tracked_markets_message,
)
from .regime import classify_regime, market_leadership
from .professional import (EventRiskCalendar, PortfolioRiskLimits, ProfessionalResearchStore,
                           portfolio_risk_check, research_validation)


def _client() -> RobinhoodReadOnlyClient:
    return RobinhoodReadOnlyClient(Settings.from_environment())


def _print_safe_account(account: dict) -> None:
    safe = {
        "status": account.get("status"),
        "buying_power": account.get("buying_power"),
        "buying_power_currency": account.get("buying_power_currency"),
        "account_number": "configured" if account.get("account_number") else None,
    }
    print(json.dumps(safe, indent=2))


PROMOTION_READY_NOTIFICATION = "promotion_gate_passed_v1"


def _send_shadow_discord(
    store: ShadowStore,
    webhook_url: str,
    actions: list[dict[str, object]],
    summary: dict[str, object],
    *,
    allow_promotion_alert: bool = True,
    track_name: str = "Strict Control",
) -> None:
    if not webhook_url:
        return
    webhook = DiscordWebhook(webhook_url)
    message = action_message(actions, summary)
    if message:
        webhook.send(f"**Track: {track_name}**\n{message}")
    milestone = promotion_ready_message(summary)
    if allow_promotion_alert and milestone and not store.notification_sent(PROMOTION_READY_NOTIFICATION):
        webhook.send(milestone)
        store.mark_notification_sent(PROMOTION_READY_NOTIFICATION)
        store.audit("SYSTEM", "promotion_ready_notified", "all promotion prerequisites passed")


def main() -> None:
    parser = argparse.ArgumentParser(description="Read-only Robinhood crypto research tools")
    subparsers = parser.add_subparsers(dest="command", required=True)
    subparsers.add_parser("health", help="Verify signed read-only account access")

    pairs = subparsers.add_parser("pairs", help="List API-tradable pairs")
    pairs.add_argument("--symbols", nargs="*", default=[])

    quote = subparsers.add_parser("quote", help="Get current and quantity-specific quotes")
    quote.add_argument("symbol")
    quote.add_argument("--quantity", required=True)

    target = subparsers.add_parser("target", help="Calculate a cost-aware target price")
    target.add_argument("--entry-price", required=True)
    target.add_argument("--entry-fee-rate", default="0")
    target.add_argument("--exit-fee-rate", default="0")
    target.add_argument("--slippage-rate", default="0")
    target.add_argument("--target-net-return", default="0.09")

    ingest = subparsers.add_parser("ingest", help="Store validated public research candles")
    ingest.add_argument("--symbols", nargs="+", default=["BTC-USD", "ETH-USD"])
    ingest.add_argument("--granularity", choices=sorted(GRANULARITY_SECONDS), default="ONE_HOUR")
    ingest.add_argument("--days", type=int, default=7)
    ingest.add_argument("--database", default="data/market.db")
    ingest.add_argument("--maximum-gaps", type=int, default=3)

    backtest = subparsers.add_parser("backtest", help="Run the auditable baseline strategy")
    backtest.add_argument("--symbols", nargs="+", default=["BTC-USD", "ETH-USD"])
    backtest.add_argument("--granularity", choices=sorted(GRANULARITY_SECONDS), default="FOUR_HOUR")
    backtest.add_argument("--database", default="data/market.db")
    backtest.add_argument("--initial-equity", default="10000")
    backtest.add_argument("--fee-rate", default="0.0095")
    backtest.add_argument("--slippage-rate", default="0.002")

    universe = subparsers.add_parser("universe", help="Rank shared research candidates")
    universe.add_argument("--limit", type=int, default=12)
    universe.add_argument("--maximum-spread", default="0.01")

    walk = subparsers.add_parser("walk-forward", help="Tune on past windows and score unseen windows")
    walk.add_argument("--symbols", nargs="+", default=["BTC-USD", "ETH-USD"])
    walk.add_argument("--granularity", choices=sorted(GRANULARITY_SECONDS), default="FOUR_HOUR")
    walk.add_argument("--database", default="data/market.db")
    walk.add_argument("--train-days", type=int, default=180)
    walk.add_argument("--test-days", type=int, default=60)
    walk.add_argument("--initial-equity", default="10000")
    walk.add_argument("--fee-rate", default="account")
    walk.add_argument("--slippage-rate", default="0.002")

    breakout = subparsers.add_parser(
        "evaluate-breakout", help="Evaluate regime-filtered breakouts with an untouched holdout"
    )
    breakout.add_argument("--symbols", nargs="+", default=["BTC-USD", "ETH-USD"])
    breakout.add_argument("--granularity", choices=sorted(GRANULARITY_SECONDS), default="FOUR_HOUR")
    breakout.add_argument("--database", default="data/market.db")
    breakout.add_argument("--train-days", type=int, default=180)
    breakout.add_argument("--test-days", type=int, default=60)
    breakout.add_argument("--holdout-days", type=int, default=90)
    breakout.add_argument("--fee-rate", default="account")
    breakout.add_argument("--slippage-rate", default="0.002")

    matrix = subparsers.add_parser(
        "target-matrix", help="Compare net targets and holding periods before the holdout"
    )
    matrix.add_argument(
        "--symbols",
        nargs="+",
        default=[
            "BTC-USD", "ETH-USD", "XRP-USD", "ZEC-USD", "SOL-USD", "LINK-USD",
            "ADA-USD", "DOGE-USD", "UNI-USD", "SUI-USD", "XLM-USD",
        ],
    )
    matrix.add_argument("--granularity", choices=sorted(GRANULARITY_SECONDS), default="FOUR_HOUR")
    matrix.add_argument("--database", default="data/market.db")
    matrix.add_argument("--targets", default="0.03,0.05,0.07,0.09")
    matrix.add_argument("--holding-days", default="3,7,14,30")
    matrix.add_argument("--train-days", type=int, default=180)
    matrix.add_argument("--test-days", type=int, default=60)
    matrix.add_argument("--holdout-days", type=int, default=90)
    matrix.add_argument("--fee-rate", default="account")
    matrix.add_argument("--slippage-rate", default="0.002")

    events = subparsers.add_parser(
        "evaluate-events", help="Evaluate fixed high-conviction event signals"
    )
    events.add_argument(
        "--symbols", nargs="+", default=[
            "BTC-USD", "ETH-USD", "XRP-USD", "ZEC-USD", "SOL-USD", "LINK-USD",
            "ADA-USD", "DOGE-USD", "UNI-USD", "SUI-USD", "XLM-USD",
        ]
    )
    events.add_argument("--granularity", choices=sorted(GRANULARITY_SECONDS), default="FOUR_HOUR")
    events.add_argument("--database", default="data/market.db")
    events.add_argument("--warmup-days", type=int, default=180)
    events.add_argument("--test-days", type=int, default=60)
    events.add_argument("--target", default="0.07")
    events.add_argument("--holding-days", type=int, default=7)
    events.add_argument("--fee-rate", default="account")
    events.add_argument("--slippage-rate", default="0.002")

    shadow = subparsers.add_parser(
        "shadow-scan", help="Record hypothetical live trades without submitting orders"
    )
    shadow.add_argument(
        "--symbols", nargs="+", default=[
            "BTC-USD", "ETH-USD", "XRP-USD", "ZEC-USD", "SOL-USD", "LINK-USD",
            "ADA-USD", "DOGE-USD", "UNI-USD", "SUI-USD", "XLM-USD",
        ]
    )
    shadow.add_argument("--granularity", choices=["FOUR_HOUR"], default="FOUR_HOUR")
    shadow.add_argument("--database", default="data/market.db")
    shadow.add_argument("--shadow-database", default="data/shadow.db")
    shadow.add_argument(
        "--strategy-profile", choices=("strict", "balanced"), default="strict",
        help="Frozen signal profile; balanced is an isolated shadow challenger",
    )
    shadow.add_argument(
        "--notionals", default="100,200,300,400",
        help="Comma-separated hypothetical dollar sizes",
    )
    shadow.add_argument("--target", default=None, help="Optional fixed target; otherwise volatility-adjusted")
    shadow.add_argument("--target-floor", default="0.03")
    shadow.add_argument("--target-ceiling", default="0.09")
    shadow.add_argument("--atr-multiple", default="2.5")
    shadow.add_argument("--stop", default="0.03")
    shadow.add_argument("--holding-days", type=int, default=7)
    shadow.add_argument("--cooldown-days", type=int, default=7)
    shadow.add_argument("--refresh-days", type=int, default=30)
    shadow.add_argument("--maximum-spread", default="0.02")
    shadow.add_argument("--maximum-price-impact", default="0.005")
    shadow.add_argument("--maximum-correlation", default="0.75")
    shadow.add_argument("--maximum-open-signals", type=int, default=3)

    subparsers.add_parser("shadow-report", help="Summarize hypothetical shadow results").add_argument(
        "--shadow-database", default="data/shadow.db"
    )
    monitor = subparsers.add_parser(
        "shadow-monitor", help="Reprice and close open shadow positions without scanning entries"
    )
    monitor.add_argument("--shadow-database", default="data/shadow.db")
    monitor.add_argument(
        "--strategy-profile", choices=("strict", "balanced"), default="strict"
    )
    subparsers.add_parser("discord-test", help="Send the tracked-market list to Discord")
    regime_report = subparsers.add_parser("regime-report", help="Classify current stored market regimes")
    regime_report.add_argument(
        "--symbols", nargs="+", default=[
            "BTC-USD", "ETH-USD", "XRP-USD", "ZEC-USD", "SOL-USD", "LINK-USD",
            "ADA-USD", "DOGE-USD", "UNI-USD", "SUI-USD", "XLM-USD",
        ]
    )
    regime_report.add_argument("--database", default="data/market.db")
    professional_report = subparsers.add_parser("professional-report", help="Report robust shadow validation")
    professional_report.add_argument("--shadow-database", default="data/shadow.db")
    dashboard = subparsers.add_parser("dashboard", help="Open the local read-only activity dashboard")
    dashboard.add_argument("--host", default="127.0.0.1")
    dashboard.add_argument("--port", type=int, default=8765)

    args = parser.parse_args()
    if args.command == "dashboard":
        from .dashboard import serve

        serve(args.host, args.port)
    elif args.command == "professional-report":
        store = ShadowStore(args.shadow_database)
        store.initialize()
        print(json.dumps({"orders_submitted": 0, **research_validation(store.independent_returns())}, indent=2))
    elif args.command == "regime-report":
        store = CandleStore(args.database)
        symbols = sorted({symbol.upper() for symbol in args.symbols} | {"BTC-USD"})
        universe = {symbol: store.load("coinbase", symbol, "FOUR_HOUR") for symbol in symbols}
        benchmark = universe["BTC-USD"]
        config = EventStrategyConfig(expected_interval_seconds=GRANULARITY_SECONDS["FOUR_HOUR"])
        breadth = market_breadth(universe, config)
        latest_breadth = breadth.get(benchmark[-1].start, Decimal("0"))
        leadership = market_leadership(benchmark, universe, latest_breadth)
        reports = []
        for symbol, candles in sorted(universe.items()):
            try:
                reports.append({"symbol": symbol, **classify_regime(candles, leadership=leadership).as_json()})
            except ValueError as exc:
                reports.append({"symbol": symbol, "status": "insufficient_data", "reason": str(exc)})
        print(json.dumps({"market_breadth": str(latest_breadth), "leadership": leadership,
                          "regimes": reports}, indent=2))
    elif args.command == "discord-test":
        settings = Settings.from_environment(require_credentials=False)
        if not settings.discord_webhook_url:
            raise SystemExit("DISCORD_WEBHOOK_URL is not configured")
        DiscordWebhook(settings.discord_webhook_url).send(
            tracked_markets_message([
                "BTC-USD", "ETH-USD", "XRP-USD", "ZEC-USD", "SOL-USD", "LINK-USD",
                "ADA-USD", "DOGE-USD", "UNI-USD", "SUI-USD", "XLM-USD",
            ])
        )
        print(json.dumps({"discord": "connected", "orders_submitted": 0}, indent=2))
    elif args.command == "health":
        _print_safe_account(_client().account())
    elif args.command == "pairs":
        print(json.dumps(_client().trading_pairs(args.symbols), indent=2))
    elif args.command == "quote":
        client = _client()
        result = {
            "best_bid_ask": client.best_bid_ask([args.symbol]),
            "estimated_price": client.estimated_price(
                args.symbol, "both", [args.quantity]
            ),
        }
        print(json.dumps(result, indent=2))
    elif args.command == "target":
        slippage = Decimal(args.slippage_rate)
        costs = RoundTripCosts(
            entry_fee_rate=Decimal(args.entry_fee_rate),
            exit_fee_rate=Decimal(args.exit_fee_rate),
            entry_slippage_rate=slippage,
            exit_slippage_rate=slippage,
        )
        price = required_exit_price(
            Decimal(args.entry_price), Decimal(args.target_net_return), costs
        )
        print(json.dumps({"required_exit_price": str(price)}, indent=2))
    elif args.command == "ingest":
        if args.days < 1:
            parser.error("--days must be at least 1")
        robinhood = _client()
        pair_payload = robinhood.trading_pairs(args.symbols)
        pair_rows = pair_payload.get("results", [])
        tradable = {
            row.get("symbol")
            for row in pair_rows
            if row.get("status") == "tradable" and row.get("is_api_tradable") is True
        }
        requested = {symbol.upper() for symbol in args.symbols}
        unavailable = sorted(requested - tradable)
        if unavailable:
            raise SystemExit(f"Not API-tradable on Robinhood: {', '.join(unavailable)}")

        end = datetime.now(timezone.utc).replace(minute=0, second=0, microsecond=0)
        start = end - timedelta(days=args.days)
        provider = CoinbasePublicMarketData()
        store = CandleStore(args.database)
        store.initialize()
        summary = []
        for symbol in sorted(requested):
            candles = provider.candles_range(
                symbol,
                args.granularity,
                start=int(start.timestamp()),
                end=int(end.timestamp()),
            )
            report = validate_candles(candles, args.granularity)
            hard_failure = bool(
                report.duplicate_timestamps
                or report.misaligned_timestamps
                or len(report.missing_intervals) > args.maximum_gaps
            )
            if hard_failure:
                raise SystemExit(
                    f"Rejected {symbol}: duplicates={len(report.duplicate_timestamps)}, "
                    f"gaps={len(report.missing_intervals)}, "
                    f"misaligned={len(report.misaligned_timestamps)}"
                )
            stored = store.upsert(
                provider.provider_name, symbol, args.granularity, candles
            )
            summary.append(
                {
                    "symbol": symbol,
                    "granularity": args.granularity,
                    "received": len(candles),
                    "stored": stored,
                    "quality": "valid" if report.is_valid else "accepted_with_gaps",
                    "gaps": len(report.missing_intervals),
                }
            )
        print(json.dumps(summary, indent=2))
    elif args.command == "backtest":
        store = CandleStore(args.database)
        costs = RoundTripCosts(
            entry_fee_rate=Decimal(args.fee_rate),
            exit_fee_rate=Decimal(args.fee_rate),
            entry_slippage_rate=Decimal(args.slippage_rate),
            exit_slippage_rate=Decimal(args.slippage_rate),
        )
        config = BacktestConfig(initial_equity=Decimal(args.initial_equity), costs=costs)
        reports = []
        for symbol in sorted({symbol.upper() for symbol in args.symbols}):
            candles = store.load("coinbase", symbol, args.granularity)
            if not candles:
                raise SystemExit(f"No stored candles for {symbol} {args.granularity}")
            signals = trend_momentum_signals(
                candles,
                StrategyConfig(
                    expected_interval_seconds=GRANULARITY_SECONDS[args.granularity]
                ),
            )
            result = run_backtest(candles, signals, config)
            reports.append(
                {
                    "symbol": symbol,
                    "granularity": args.granularity,
                    "candles": len(candles),
                    "signals": len(signals),
                    "assumptions": {
                        "target_net_return": str(config.target_net_return),
                        "fee_rate_each_side": str(costs.entry_fee_rate),
                        "slippage_each_side": str(costs.entry_slippage_rate),
                        "risk_per_trade": str(config.risk_per_trade),
                        "maximum_allocation": str(config.maximum_allocation),
                    },
                    "performance": result_summary(result),
                }
            )
        print(json.dumps(reports, indent=2))
    elif args.command == "universe":
        robinhood = _client()
        pair_payload = robinhood.trading_pairs()
        pairs = pair_payload.get("results", [])
        symbols = [
            row["symbol"]
            for row in pairs
            if row.get("status") == "tradable" and row.get("is_api_tradable") is True
        ]
        quotes = robinhood.best_bid_ask(symbols).get("results", [])
        products = CoinbasePublicMarketData().products()
        candidates = select_candidates(
            pairs,
            products,
            quotes,
            maximum_spread=Decimal(args.maximum_spread),
            limit=args.limit,
        )
        print(json.dumps([candidate.as_json() for candidate in candidates], indent=2))
    elif args.command == "walk-forward":
        if args.train_days < 1 or args.test_days < 1:
            parser.error("training and test days must be positive")
        fee_rate = (
            _client().current_fee_rate()
            if args.fee_rate == "account"
            else Decimal(args.fee_rate)
        )
        costs = RoundTripCosts(
            entry_fee_rate=fee_rate,
            exit_fee_rate=fee_rate,
            entry_slippage_rate=Decimal(args.slippage_rate),
            exit_slippage_rate=Decimal(args.slippage_rate),
        )
        config = BacktestConfig(initial_equity=Decimal(args.initial_equity), costs=costs)
        interval = GRANULARITY_SECONDS[args.granularity]
        bars_per_day = 86400 // interval
        store = CandleStore(args.database)
        reports = []
        for symbol in sorted({symbol.upper() for symbol in args.symbols}):
            candles = store.load("coinbase", symbol, args.granularity)
            try:
                result = walk_forward(
                    candles,
                    train_bars=args.train_days * bars_per_day,
                    test_bars=args.test_days * bars_per_day,
                    interval_seconds=interval,
                    backtest_config=config,
                )
            except ValueError as exc:
                reports.append({"symbol": symbol, "status": "insufficient_data", "reason": str(exc)})
                continue
            reports.append(
                {
                    "symbol": symbol,
                    "status": "evaluated",
                    "fee_rate_each_side": str(fee_rate),
                    **walk_forward_summary(result),
                }
            )
        print(json.dumps(reports, indent=2))
    elif args.command == "evaluate-breakout":
        fee_rate = (
            _client().current_fee_rate()
            if args.fee_rate == "account"
            else Decimal(args.fee_rate)
        )
        costs = RoundTripCosts(
            fee_rate,
            fee_rate,
            Decimal(args.slippage_rate),
            Decimal(args.slippage_rate),
        )
        backtest_config = BacktestConfig(costs=costs)
        interval = GRANULARITY_SECONDS[args.granularity]
        bars_per_day = 86400 // interval
        store = CandleStore(args.database)
        benchmark = store.load("coinbase", "BTC-USD", args.granularity)
        reports = []
        for symbol in sorted({symbol.upper() for symbol in args.symbols}):
            candles = store.load("coinbase", symbol, args.granularity)
            try:
                result = evaluate_breakout_with_holdout(
                    candles,
                    benchmark,
                    train_bars=args.train_days * bars_per_day,
                    test_bars=args.test_days * bars_per_day,
                    holdout_bars=args.holdout_days * bars_per_day,
                    interval_seconds=interval,
                    backtest_config=backtest_config,
                    is_benchmark=symbol == "BTC-USD",
                )
                reports.append({"symbol": symbol, "status": "evaluated", **breakout_summary(result)})
            except ValueError as exc:
                reports.append({"symbol": symbol, "status": "insufficient_data", "reason": str(exc)})
        print(json.dumps(reports, indent=2))
    elif args.command == "target-matrix":
        fee_rate = (
            _client().current_fee_rate()
            if args.fee_rate == "account"
            else Decimal(args.fee_rate)
        )
        costs = RoundTripCosts(
            fee_rate,
            fee_rate,
            Decimal(args.slippage_rate),
            Decimal(args.slippage_rate),
        )
        targets = tuple(Decimal(value.strip()) for value in args.targets.split(","))
        holding_days = tuple(int(value.strip()) for value in args.holding_days.split(","))
        interval = GRANULARITY_SECONDS[args.granularity]
        bars_per_day = 86400 // interval
        store = CandleStore(args.database)
        candles_by_symbol = {
            symbol.upper(): store.load("coinbase", symbol.upper(), args.granularity)
            for symbol in args.symbols
        }
        cells = evaluate_target_horizon_matrix(
            candles_by_symbol,
            targets=targets,
            holding_days=holding_days,
            train_bars=args.train_days * bars_per_day,
            test_bars=args.test_days * bars_per_day,
            holdout_bars=args.holdout_days * bars_per_day,
            bars_per_day=bars_per_day,
            interval_seconds=interval,
            costs=costs,
        )
        print(json.dumps(matrix_summary(cells), indent=2))
    elif args.command == "evaluate-events":
        fee_rate = (
            _client().current_fee_rate()
            if args.fee_rate == "account"
            else Decimal(args.fee_rate)
        )
        interval = GRANULARITY_SECONDS[args.granularity]
        bars_per_day = 86400 // interval
        strategy_config = EventStrategyConfig(expected_interval_seconds=interval)
        backtest_config = BacktestConfig(
            target_net_return=Decimal(args.target),
            maximum_holding_bars=args.holding_days * bars_per_day,
            costs=RoundTripCosts(
                fee_rate,
                fee_rate,
                Decimal(args.slippage_rate),
                Decimal(args.slippage_rate),
            ),
        )
        store = CandleStore(args.database)
        universe = {
            symbol.upper(): store.load("coinbase", symbol.upper(), args.granularity)
            for symbol in args.symbols
        }
        benchmark = store.load("coinbase", "BTC-USD", args.granularity)
        reports = []
        for symbol, candles in sorted(universe.items()):
            try:
                result = evaluate_event_folds(
                    symbol,
                    candles,
                    benchmark,
                    universe,
                    warmup_bars=args.warmup_days * bars_per_day,
                    test_bars=args.test_days * bars_per_day,
                    strategy_config=strategy_config,
                    backtest_config=backtest_config,
                )
                reports.append({"symbol": symbol, "status": "evaluated", **event_summary(result)})
            except ValueError as exc:
                reports.append({"symbol": symbol, "status": "insufficient_data", "reason": str(exc)})
        print(json.dumps(reports, indent=2))
    elif args.command == "shadow-monitor":
        robinhood = _client()
        store = ShadowStore(args.shadow_database)
        store.initialize()
        now_timestamp = int(datetime.now(timezone.utc).timestamp())
        actions = []
        by_symbol: dict[str, list] = {}
        for position in store.open_positions():
            by_symbol.setdefault(position.symbol, []).append(position)
        for symbol, positions in by_symbol.items():
            quantities = [position.quantity for position in positions]
            prices = batched_execution_prices(
                robinhood.estimated_price(symbol, "both", [str(value) for value in quantities]),
                quantities,
            )
            for position, (_, bid) in zip(positions, prices):
                store.mark_position(position.id, bid, now_timestamp)
                reason = None
                if bid <= position.stop_price:
                    reason = "stop"
                elif bid >= position.target_price:
                    reason = "target"
                elif now_timestamp >= position.expires_at:
                    reason = "expired"
                if reason:
                    store.close_position(position, bid, reason)
                    store.audit(symbol, "closed", f"{reason}; bid={bid}; five_minute_monitor")
                    actions.append({"symbol": symbol, "action": "closed", "reason": reason})
        summary = store.summary()
        settings = Settings.from_environment()
        _send_shadow_discord(
            store,
            settings.discord_webhook_url,
            actions,
            summary,
            allow_promotion_alert=args.strategy_profile == "strict",
            track_name=(
                "Strict Control"
                if args.strategy_profile == "strict"
                else "Balanced Challenger"
            ),
        )
        print(json.dumps({"orders_submitted": 0, "actions": actions, **summary}, indent=2))
    elif args.command == "shadow-scan":
        if args.holding_days < 1 or args.refresh_days < 1 or args.cooldown_days < 0:
            parser.error("holding and refresh days must be positive; cooldown cannot be negative")
        try:
            notionals = tuple(
                sorted({Decimal(value.strip()) for value in args.notionals.split(",")})
            )
        except Exception:
            parser.error("notionals must be comma-separated numbers")
        fixed_target = None if args.target is None else Decimal(args.target)
        target_floor = Decimal(args.target_floor)
        target_ceiling = Decimal(args.target_ceiling)
        atr_multiple = Decimal(args.atr_multiple)
        stop_rate = Decimal(args.stop)
        maximum_spread = Decimal(args.maximum_spread)
        maximum_price_impact = Decimal(args.maximum_price_impact)
        maximum_correlation = Decimal(args.maximum_correlation)
        if (
            not notionals or len(notionals) > 10 or min(notionals) <= 0 or target_floor <= 0
            or target_ceiling < target_floor or atr_multiple <= 0
            or fixed_target is not None and fixed_target <= 0
            or not 0 < stop_rate < 1 or args.maximum_open_signals < 1
            or maximum_spread < 0 or maximum_price_impact < 0
            or not Decimal("-1") <= maximum_correlation <= Decimal("1")
        ):
            parser.error("invalid sizing, target, stop, or exposure setting")

        symbols = sorted({symbol.upper() for symbol in args.symbols})
        if "BTC-USD" not in symbols:
            symbols.append("BTC-USD")
        robinhood = _client()
        fee_rate = robinhood.current_fee_rate()
        shadow_store = ShadowStore(args.shadow_database)
        shadow_store.initialize()
        professional_store = ProfessionalResearchStore("data/professional.db")
        professional_store.initialize()
        event_calendar = EventRiskCalendar("data/event-risk.json")
        market_store = CandleStore(args.database)
        market_store.initialize()

        now = datetime.now(timezone.utc)
        interval = GRANULARITY_SECONDS[args.granularity]
        completed_cutoff = int(now.timestamp()) // interval * interval
        provider = CoinbasePublicMarketData()
        start = int((now - timedelta(days=args.refresh_days)).timestamp())
        for symbol in symbols:
            fresh = [
                candle for candle in provider.candles_range(
                    symbol, args.granularity, start=start, end=completed_cutoff
                )
                if candle.start < completed_cutoff
            ]
            if fresh:
                market_store.upsert("coinbase", symbol, args.granularity, fresh)
            try:
                professional_store.record_book(symbol, provider.order_book(symbol))
            except Exception as exc:
                shadow_store.audit(symbol, "order_book_unavailable", str(exc))

        universe = {
            symbol: market_store.load("coinbase", symbol, args.granularity)
            for symbol in symbols
        }

        def benchmark_returns(started_at: int, ended_at: int, rate: Decimal) -> tuple[Decimal, Decimal]:
            costs = RoundTripCosts(rate, rate)
            values = []
            btc_value = None
            for market_symbol, candles in universe.items():
                entry = [candle for candle in candles if candle.start <= started_at]
                exit_ = [candle for candle in candles if candle.start <= ended_at]
                if not entry or not exit_:
                    continue
                value = net_return(entry[-1].close, exit_[-1].close, costs)
                values.append(value)
                if market_symbol == "BTC-USD":
                    btc_value = value
            if btc_value is None or not values:
                return Decimal("0"), Decimal("0")
            return btc_value, sum(values, Decimal("0")) / Decimal(len(values))

        actions: list[dict[str, object]] = []
        open_by_symbol: dict[str, list] = {}
        for position in shadow_store.open_positions():
            open_by_symbol.setdefault(position.symbol, []).append(position)
        for symbol, positions in open_by_symbol.items():
            quantities = [position.quantity for position in positions]
            estimates = batched_execution_prices(
                robinhood.estimated_price(symbol, "both", [str(value) for value in quantities]),
                quantities,
            )
            for position, (_, bid) in zip(positions, estimates):
                shadow_store.mark_position(position.id, bid, int(now.timestamp()))
                reason = None
                if bid <= position.stop_price:
                    reason = "stop"
                elif bid >= position.target_price:
                    reason = "target"
                elif int(now.timestamp()) >= position.expires_at:
                    reason = "expired"
                if reason:
                    btc_return, market_return = benchmark_returns(
                        position.opened_at, int(now.timestamp()), position.fee_rate
                    )
                    shadow_store.close_position(
                        position, bid, reason,
                        btc_benchmark_return=btc_return,
                        market_benchmark_return=market_return,
                    )
                    shadow_store.audit(position.symbol, "closed", f"{reason}; bid={bid}")
                    actions.append({"symbol": position.symbol, "action": "closed", "reason": reason})

        for position_id, opened_at, closed_at, rate in shadow_store.positions_missing_benchmarks():
            btc_return, market_return = benchmark_returns(opened_at, closed_at, rate)
            shadow_store.set_benchmarks(position_id, btc_return, market_return)
        benchmark = universe["BTC-USD"]
        config = event_strategy_profile(
            args.strategy_profile, expected_interval_seconds=interval
        )
        breadth = market_breadth(universe, config)
        latest_breadth = breadth.get(benchmark[-1].start, Decimal("0"))
        leadership = market_leadership(benchmark, universe, latest_breadth)
        try:
            broad_regime = classify_regime(benchmark, leadership=leadership, expected_interval=interval)
        except ValueError as exc:
            shadow_store.audit("BTC-USD", "regime_veto", str(exc))
            broad_regime = None
        for symbol, candles in sorted(universe.items()):
            event_reasons = event_calendar.active_reasons(int(now.timestamp()), symbol)
            if event_reasons:
                shadow_store.audit(symbol, "event_risk_veto", "; ".join(event_reasons))
                continue
            if len(candles) <= config.warmup:
                continue
            signals = high_conviction_event_signals(
                candles, benchmark, breadth, config, is_benchmark=symbol == "BTC-USD"
            )
            latest = signals[-1] if signals and signals[-1].candle_index == len(candles) - 1 else None
            if latest is None:
                shadow_store.audit(symbol, "no_trade", "no current high-conviction event")
                continue
            professional_store.focus(symbol, int(now.timestamp()) + 6 * 3600)
            stream_ok, stream_reason = professional_store.flow_health(symbol, int(now.timestamp()))
            if not stream_ok:
                shadow_store.audit(symbol, "stream_veto", stream_reason)
                continue
            try:
                asset_regime = classify_regime(
                    candles, leadership=leadership, expected_interval=interval
                )
            except ValueError as exc:
                shadow_store.audit(symbol, "regime_veto", str(exc))
                continue
            if broad_regime is None or not broad_regime.permits_long_breakout:
                reasons = "unavailable" if broad_regime is None else ",".join(broad_regime.reasons)
                shadow_store.audit(symbol, "regime_veto", f"market:{reasons}")
                continue
            if not asset_regime.permits_long_breakout:
                shadow_store.audit(symbol, "regime_veto", ",".join(asset_regime.reasons))
                continue
            if not shadow_store.signal_allowed(
                symbol, latest.timestamp, args.cooldown_days * 86400
            ):
                shadow_store.audit(symbol, "cooldown", f"signal={latest.timestamp}")
                continue
            active_symbols = {position.symbol for position in shadow_store.open_positions()}
            if len(active_symbols) >= args.maximum_open_signals:
                shadow_store.audit(symbol, "risk_veto", "maximum open signals reached")
                continue
            correlated = []
            for active_symbol in active_symbols:
                value = return_correlation(candles, universe[active_symbol])
                if value is not None and value >= maximum_correlation:
                    correlated.append(f"{active_symbol}:{value:.3f}")
            if correlated:
                shadow_store.audit(symbol, "correlation_veto", ",".join(correlated))
                continue
            composite = execution_price(robinhood.best_bid_ask([symbol]), "ask")
            quantities = [quantity_for_notional(size, composite) for size in notionals]
            prices = batched_execution_prices(
                robinhood.estimated_price(symbol, "both", [str(value) for value in quantities]),
                quantities,
            )
            liquid, quality = execution_quality(
                prices,
                maximum_spread=maximum_spread,
                maximum_price_impact=maximum_price_impact,
            )
            if not liquid:
                shadow_store.audit(
                    symbol, "liquidity_veto",
                    f"spread={quality['maximum_spread']}; impact={quality['ask_price_impact']}",
                )
                continue
            open_notional, daily_return, consecutive_losses = shadow_store.portfolio_risk_state()
            risk_ok, risk_reasons = portfolio_risk_check(
                open_notional=open_notional, proposed_notional=max(notionals),
                daily_return=daily_return, consecutive_losses=consecutive_losses,
                limits=PortfolioRiskLimits(),
            )
            if not risk_ok:
                shadow_store.audit(symbol, "portfolio_risk_veto", "; ".join(risk_reasons))
                continue
            target_return = fixed_target or volatility_target(
                candles, multiplier=atr_multiple, floor=target_floor, ceiling=target_ceiling
            )
            rationale = (
                f"profile={args.strategy_profile}; {latest.reason}; "
                f"regime={asset_regime.trend}/{asset_regime.phase}; "
                f"volatility={asset_regime.volatility}; leadership={leadership}; daily_confirmed"
            )
            for requested_notional, quantity, (ask, bid) in zip(notionals, quantities, prices):
                professional_store.record_execution(symbol, latest.timestamp, quantity, ask, bid,
                                                     quality["ask_price_impact"])
                professional_store.simulate_order(symbol, latest.timestamp, quantity, "filled",
                                                   "shadow fill only; no order submitted")
                target_price, stop_price = shadow_levels(ask, target_return, stop_rate, fee_rate)
                opened = shadow_store.open_position(
                    symbol=symbol,
                    signal_timestamp=latest.timestamp,
                    entry_price=ask,
                    quantity=quantity,
                    requested_notional=requested_notional,
                    target_price=target_price,
                    stop_price=stop_price,
                    expires_at=int(now.timestamp()) + args.holding_days * 86400,
                    fee_rate=fee_rate,
                    rationale=rationale,
                )
                action = "opened" if opened else "duplicate_signal"
                shadow_store.audit(
                    symbol, action, f"size={requested_notional}; {rationale}"
                )
                if opened:
                    actions.append({
                        "symbol": symbol,
                        "action": "opened_experimental_shadow",
                        "requested_notional": str(requested_notional),
                        "estimated_notional": str((ask * quantity).quantize(Decimal("0.01"))),
                        "estimated_entry_price": str(ask),
                        "target_net_return": str(target_return),
                        "maximum_net_loss": str(stop_rate),
                        "spread": str(quality["maximum_spread"]),
                        "price_impact": str(quality["ask_price_impact"]),
                        "regime": f"{asset_regime.trend}/{asset_regime.phase}",
                        "leadership": leadership,
                        "strategy_profile": args.strategy_profile,
                    })
        summary = shadow_store.summary()
        _send_shadow_discord(
            shadow_store, robinhood.settings.discord_webhook_url, actions, summary,
            allow_promotion_alert=args.strategy_profile == "strict",
            track_name=(
                "Strict Control"
                if args.strategy_profile == "strict"
                else "Balanced Challenger"
            ),
        )
        print(json.dumps({"orders_submitted": 0, "actions": actions, **summary}, indent=2))
    elif args.command == "shadow-report":
        store = ShadowStore(args.shadow_database)
        store.initialize()
        print(json.dumps({"orders_submitted": 0, **store.summary()}, indent=2))


if __name__ == "__main__":
    main()
