## 🌍 2. **CDN / Reverse Proxy Caching**

This is where the biggest performance gains happen.

Use a CDN like Cloudflare, Akamai, or AWS CloudFront to cache GET responses **at the edge**.

### Benefits

- Offloads 80–95% of traffic
- Reduces latency dramatically
- Protects your origin servers from spikes

### How it works

CDN respects your `Cache-Control` headers.  
If the resource is cached, the CDN returns it instantly.

## 🧠 4. **Server-Side Cache (Redis)**

When the CDN misses, your backend should still avoid hitting the database.

Use Redis for:

- **hot data caching**
- **computed response caching**
- **query result caching**

### Example

Key: `order:12345`  
TTL: 30–120 seconds depending on volatility

Redis gives you **sub‑millisecond** reads.

## 🔥 5. **Cache Invalidation Strategy**

This is the hardest part.

### Options

- **TTL-based invalidation** — simplest, eventual consistency
- **Event-driven invalidation** — publish events when data changes
- **Versioned keys** — embed version number in cache key
- **ETag regeneration** — new ETag = new version

### Example

When an order is updated:

- Invalidate Redis key
- Regenerate ETag
- CDN sees new ETag → fetches fresh version

## 📏 6. **Choosing TTL**

TTL depends on how dynamic the data is.

- **Highly dynamic** → 5–30 seconds
- **Moderately dynamic** → 30–120 seconds
- **Static content** → hours or days

Short TTL + ETags = fast + safe.
