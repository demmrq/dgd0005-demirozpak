# dgd0005-demirozpak



\## Step 10 — Bytes and tick rate



\### Measurements

| Tick Rate | Server out B/s | Visual smoothness (ball and paddles) |

| :---: | :---: | :--- |

| \*\*10\*\* | \~1420 B/s | Choppy and stuttering. Ball and paddles exhibit visible jitter between frames. |

| \*\*60\*\* | \~7850 B/s | Extremely smooth movement with no visible stutter, but network bandwidth usage increases nearly 5-6x. |

| \*\*30\*\* | \~3800 B/s | Balanced and smooth. Good visual quality with reasonable bandwidth consumption. |



\### Analysis Questions

\- \*\*Did the bytes/sec change roughly linearly with tick rate?\*\*

&#x20; Yes, the bandwidth usage scales nearly linearly with the tick rate because NetworkTransform synchronizes object positions and state updates once per tick.



\- \*\*Was your prediction in Step 7 accurate?\*\*

&#x20; Yes, lowering the tick rate reduced network traffic significantly at the cost of visual smoothness, while increasing it provided higher precision at the expense of higher throughput.

