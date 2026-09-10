import asyncio
import json

import websockets


async def main():
    symbols = ["BTC-USD", "ETH-USD", "XRP-USD"]
    async with websockets.connect(
        "wss://advanced-trade-ws.coinbase.com", max_size=8 * 1024 * 1024
    ) as socket:
        for channel in ("level2", "market_trades", "heartbeats"):
            await socket.send(json.dumps({
                "type": "subscribe", "product_ids": symbols, "channel": channel
            }))
        for _ in range(40):
            payload = json.loads(await asyncio.wait_for(socket.recv(), timeout=10))
            events = payload.get("events", [])
            products = sorted({str(event.get("product_id", "")) for event in events})
            types = sorted({str(event.get("type", "")) for event in events})
            print(payload.get("channel"), payload.get("sequence_num"), products, types)


asyncio.run(main())
