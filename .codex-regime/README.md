# Robinhood Crypto Recommendation Bot

A security-first foundation for researching and paper-trading crypto strategies whose
objective is a **9% net return after estimated costs**. The target is not a guarantee.

The current release is deliberately read-only. It can authenticate with Robinhood, inspect
the account and tradable pairs, retrieve quotes, and calculate cost-aware target prices. It
does **not** contain an order-placement method.

## Safety boundary

- Spot crypto only; no leverage or derivatives.
- API credentials should initially receive read-only actions.
- Withdrawal capability must remain disabled.
- `.env` is ignored by Git and must never be committed.
- A recommendation may be produced only after quantity-specific Robinhood pricing is checked.

## Setup

1. Install Python 3.12 or later.
2. Create a virtual environment: `python -m venv .venv`.
3. Activate it and install the project: `python -m pip install -e ".[dev]"`.
4. Copy `.env.example` to `.env` and insert read-only Robinhood API credentials.
5. Run `rh-crypto-bot health`.
6. Run tests with `python -m pytest`.

Environment values may also be supplied by a deployment secret manager instead of `.env`.
The application loads `.env` only as a local convenience and never prints the private key.

## Commands

```text
rh-crypto-bot health
rh-crypto-bot pairs --symbols BTC-USD ETH-USD
rh-crypto-bot quote BTC-USD --quantity 0.001
rh-crypto-bot target --entry-price 100 --entry-fee-rate 0.0095 \
  --exit-fee-rate 0.0095 --slippage-rate 0.002
rh-crypto-bot ingest --symbols BTC-USD ETH-USD --granularity ONE_HOUR --days 7
rh-crypto-bot backtest --symbols BTC-USD ETH-USD --granularity FOUR_HOUR
rh-crypto-bot universe --limit 12 --maximum-spread 0.01
rh-crypto-bot walk-forward --symbols BTC-USD ETH-USD --train-days 180 --test-days 60
rh-crypto-bot evaluate-breakout --symbols BTC-USD ETH-USD --holdout-days 90
rh-crypto-bot target-matrix --targets 0.03,0.05,0.07,0.09 --holding-days 3,7,14,30
rh-crypto-bot evaluate-events --target 0.07 --holding-days 7
rh-crypto-bot shadow-scan --notionals 100,200,300,400 --target-floor 0.03 \
  --target-ceiling 0.09 --holding-days 7 --cooldown-days 7
rh-crypto-bot shadow-report
rh-crypto-bot shadow-monitor
rh-crypto-bot discord-test
rh-crypto-bot regime-report
rh-crypto-bot dashboard
```

## Local activity dashboard

`rh-crypto-bot dashboard` starts a read-only command center at
`http://127.0.0.1:8765`. It refreshes every 15 seconds and shows connection activity,
scheduled-task health, shadow profit/loss, research-gate progress, open observations,
markets under consideration, recent decisions, sizing results, and upcoming event risk.
It binds only to this computer, reads the existing local databases, exposes no credentials
or account identifiers, and contains no order controls.

## Project status

Historical candles come from Coinbase's unauthenticated public market-data API and are used
only for research. Robinhood remains authoritative for tradability, fees, and executable
prices. The initial importer supports one-hour and four-hour candles, validates continuity,
and stores normalized decimal values in a local SQLite database under the ignored `data/`
directory. Long ranges are automatically divided into provider-sized pages and de-duplicated.

The importer rejects duplicates, timestamp misalignment, and excessive gaps. By default it may
accept up to three explicitly reported provider gaps; candles are never fabricated. Strategy
warm-up resets across every gap so indicators and signals cannot treat discontinuous periods as
adjacent observations. Set `--maximum-gaps 0` for strict ingestion.

The `universe` command traverses all Robinhood product pages, intersects API-tradable pairs
with available Coinbase USD spot products, rejects common stablecoins and spreads above the
configured ceiling, then ranks the remainder by estimated Coinbase 24-hour USD notional volume. This ranking is
a research filter, not a trade recommendation.

Robinhood's cross-exchange best-price composite can be slightly crossed. Small negative
composite spreads are treated as zero during universe filtering; every eventual recommendation
must still pass Robinhood's quantity-specific estimated-price cost check.

See `ROADMAP.md`. The next implementation stage is deterministic backtesting and paper trading.

## Backtest baseline

The initial long-only baseline combines fast/slow moving averages, six-bar momentum, and
volume confirmation. Signals use completed candles and enter at the next candle's open. Tests
assume a conservative fee and slippage charge on both entry and exit, a 9% net target, a 3%
stop, 0.5% account risk, at most 20% allocation, and stop-first ordering when a single candle
touches both stop and target. These assumptions are intentionally pessimistic and configurable.

Backtest results are research evidence, not forecasts or promises of future performance.

The `walk-forward` command selects from a small, declared parameter set using each trailing
training window, then evaluates the selected configuration on the next unseen window. By
default it retrieves the current Robinhood account fee rate and uses 0.2% slippage on each
side. Fold results remain separate and report parameter stability, unseen trades, compounded
test return, profitable folds, and worst fold drawdown.

No result may advance to recommendation research unless it has at least three unseen folds,
30 unseen trades, profitable performance in at least two-thirds of folds, positive compounded
unseen return, and no fold drawdown above 5%. Passing this gate would justify further testing;
it would not authorize paper or live trading.

The breakout evaluator requires price to clear a prior range on above-average volume, permits
long entries only during a bullish non-extreme BTC regime, and requires non-BTC assets to
outperform BTC over the configured lookback. Candidate parameters are selected before the most
recent 90-day holdout is revealed. The holdout is evaluated once and is never used for tuning.

The target matrix compares net-return objectives and maximum holding periods using only data
before the protected holdout. It reports mean and median unseen-window results across assets,
trade counts, drawdown, and research-gate passes. It does not inspect the holdout or select a
production setting automatically.

The fixed event strategy is intentionally sparse: it requires a range breakout, 1.5x true-range
expansion, 1.5x volume, a positive daily-equivalent trend in both the asset and BTC, at least
60% positive market breadth, and 3% relative outperformance versus BTC for non-BTC assets.
Thresholds are declared once and evaluated in rolling 60-day blocks without parameter tuning.

## Experimental shadow monitoring

The scheduled scan now runs two fully isolated paper-research tracks. **Strict Control** keeps
the original high-conviction thresholds and remains the only track eligible to trigger a manual
promotion-review notice. **Balanced Challenger** stores results in `data/shadow-balanced.db` and
modestly widens only the breakout, expansion, volume, breadth, trend, and relative-strength
filters. It retains the same data freshness, event-risk, order-flow, liquidity, correlation,
portfolio-risk, stop-loss, completed-candle, and no-live-order controls. Challenger observations
never merge into the control sample and cannot enable recommendations or trading.

`shadow-scan` refreshes completed four-hour Coinbase candles, evaluates the fixed event rules,
and records parallel $100, $200, $300, and $400 hypothetical positions using a separate
Robinhood quantity-specific spread-inclusive estimate for each size.
Each scan reprices open positions and records a target, stop, or expiration exit. Results and an
append-only decision log are stored in ignored `data/shadow.db`; `shadow-report` summarizes them.
The report groups entry prices, exit prices, percentage returns, and dollar profit or loss by size
so any execution degradation can be measured rather than assumed.

Signal evidence is counted once using the smallest tracked size, so four sizing variants cannot
inflate the sample count. Reports also show results by coin, marked-to-market returns for open
positions, and fee-adjusted BTC and equal-weight-market comparisons. A seven-day per-coin cooldown
and one-open-signal rule prevent rapid re-entry.

The statistical promotion gate requires at least 25 independent closed signals, a positive lower
bound on the 95% confidence interval, and average performance above cash, fee-adjusted BTC, and a
fee-adjusted equal-weight market benchmark. Even a passing result leaves recommendations disabled;
enabling them requires a separate manual review and code change.

## Market-regime research

`regime-report` classifies every tracked coin as bullish, bearish, or sideways; normal or high
volatility; and early, established, pullback, exhausted, reversal-risk, declining, or
consolidating. It confirms the four-hour trend against a derived daily timeframe and labels
market leadership as Bitcoin-led, broad-altcoin, mixed, or risk-off. Discontinuous recent data
is rejected. New long shadow entries require supportive broad-market and coin regimes; every
regime veto is retained in the audit history for later outcome research.

The lightweight `shadow-monitor` command reprices every open size in one batched Robinhood
request per coin and evaluates exits without downloading candle history or looking for entries.
It is intended to run every five minutes while the full signal scan remains on completed
four-hour candles. New entries are rejected when spread exceeds 2%, size-related ask impact
exceeds 0.5%, three independent signals are already open, or correlation to an open coin is
0.75 or greater. Targets vary between 3% and 9% net using recent true-range volatility, and
stops are solved so the modeled loss is 3% net after fees rather than 3% in raw price.

When `DISCORD_WEBHOOK_URL` is configured, `discord-test` posts the complete tracked-market
list, separating BTC as the benchmark from the altcoins. Shadow scans post sanitized messages only when
positions open or close. Mentions are disabled, the destination must be an official Discord
HTTPS webhook, and messages exclude credentials, account identifiers, and balances. The webhook
is optional and stored only in the ignored `.env` or a deployment secret manager.

The local scan and monitor also send a one-time milestone message when every promotion-gate
requirement passes. The marker is persisted in the ignored shadow database so scheduled runs do not
repeat the alert. This notification requests manual review; it never enables recommendations or live orders.
The gate requires at least 25 independent signals, a 0.25% average net return, profit factor of at
least 1.25, maximum compounded-equity drawdown of 15% or less, a positive 95% confidence lower
bound, and outperformance of the fee-adjusted BTC and equal-weight market benchmarks. Fifty
independent signals are reported as the preferred sample before an automated-trading review.

## Local scheduling without model usage

`tools/run-shadow-task.ps1` runs the deterministic Python monitor without Codex or another
language model. Windows Task Scheduler should run `monitor` every five minutes and `scan` every
four hours. The runner prevents overlapping executions, times out stalled work, redacts Discord
webhook URLs from logs, stores dated logs in ignored `data/logs/`, and attempts a fixed sanitized
Discord warning when a task fails. Normal open/close notifications continue through the bot.

A separate daily `health` task uses `DISCORD_HEALTH_WEBHOOK_URL` to verify signed Robinhood
access and successful Discord delivery. Its message includes the latest locally recorded scan
and monitor results, independent-signal count, open shadow positions, and promotion-gate state.
Operational status is stored in the ignored `data/scheduler-status.db` and uses no model tokens.

This mode submits zero orders. Because the event strategy failed its historical research gate,
its entries are experimental observations—not recommendations. Run the scan after each completed
four-hour candle for forward evidence; do not use its output to place trades.

## Professional research controls

Each four-hour scan now records Coinbase Level-2 order-book depth and imbalance plus the
quantity-specific Robinhood bid, ask, spread, and size impact in ignored `data/professional.db`.
New shadow entries also pass portfolio-capital, daily-loss, consecutive-loss, and configured
event-risk circuit breakers. Copy `event-risk.example.json` to ignored `data/event-risk.json`
and replace its example with reviewed ISO-8601 UTC event windows; missing files mean no event vetoes.
Timestamps must include `Z` or an explicit UTC offset. The local calendar is pre-populated from
official Federal Reserve and BLS schedules, but dates should be reviewed periodically because
agencies can revise release calendars.
`DISCORD_CALENDAR_WEBHOOK_URL` provides a separate event channel. A daily local `calendar`
task posts the full future schedule when its contents change and sends each event one reminder
when it enters the next 48 hours. Notification markers are local and prevent repeat messages.

`professional-report` adds deterministic bootstrap confidence, Monte Carlo drawdown, and
largest-winner concentration analysis to the independent forward signals. Every accepted shadow
entry also creates an idempotent simulated order-lifecycle record. This framework has no network
method for placing, cancelling, or replacing a Robinhood order and cannot trade real funds.

`rh_crypto_bot.order_flow` maintains Coinbase Level-2, market-trade, and heartbeat subscriptions
for all tracked markets. Coinbase reports maker side, so the collector inverts it to store
aggressive buyer and seller notional. It validates the connection-wide feed sequence across
interleaved channels and products, ignores
duplicate or late messages, and reconnects for clean snapshots after a gap. No flow row is emitted
before a full Level-2 snapshot. Stale, recovering, or unavailable flow vetoes entry. The corrected
feature version discards incompatible derived minute history once. Coinbase remains research
data—fresh Robinhood estimates remain the final execution authority.
