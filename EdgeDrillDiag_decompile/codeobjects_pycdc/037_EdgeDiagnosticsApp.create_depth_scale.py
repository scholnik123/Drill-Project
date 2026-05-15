# Source Generated with Decompyle++
# File: 037_EdgeDiagnosticsApp.create_depth_scale.pyc (Python 3.12)

max_depth = 6000
segments = [
    (145, 0, 0),
    (145, 0, -max_depth)]
for depth in range(0, max_depth + 1, 500):
    tick = 28 if depth % 1000 == 0 else 16
    segments.extend([
        (145 - tick, 0, -depth),
        (145 + tick, 0, -depth)])
self.create_lines(segments, (17, 17, 17, 150), width = 1.5)
