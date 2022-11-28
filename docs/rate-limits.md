# Rate limits

Rate limits are defined by each Mastodon server, and may vary. Sensible defaults are defined here: https://docs.joinmastodon.org/api/rate-limits/

Consensus Chess nodes attempt to be good citizens by rate limiting themselves. To this end, they track usage of the APIs with a `RateLimiter` object, and self-limit (adding delays to communications) if they exceed their allotted usage.

Usage is defined as:

* `int permitted` - number of permitted requests in the time period
* `TimeSpan period` - period within which number of requests is measured

Mastonet does not (yet) expose [rate limiting headers](https://docs.joinmastodon.org/api/rate-limits/) from the Mastodon API. I've filed issue [#85 - expose rate-limiting headers](https://github.com/glacasa/Mastonet/issues/85) to ask about it.

## Monitoring

If an instance is rate-limiting itself, it'll log a warning message to that effect, and delay the delivery of its message:

```
Rate limit exceeded... Introducing a delay.
```

Rate limiting is a synchronous activity. If one message is delayed, all other messages are delayed too.

## Metrics

At current time, there's no queue or management for delayed messages, nor is there a metric to help track the number of currently delayed messages.

See:

* [ICG-75 - Implement message and API activity queues to better manage and observe rate limits](https://trello.com/c/2hNTHIIC/75-implement-message-and-api-activity-queues-to-better-manage-and-observe-rate-limits)
