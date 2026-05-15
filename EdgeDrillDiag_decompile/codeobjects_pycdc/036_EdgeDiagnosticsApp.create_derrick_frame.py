# Source Generated with Decompyle++
# File: 036_EdgeDiagnosticsApp.create_derrick_frame.pyc (Python 3.12)

levels = [
    0,
    0.25,
    0.5,
    0.75,
    1]
rings = []
for level in levels:
    half = base_half + (top_half - base_half) * level
    z = z0 + height * level
    rings.append([
        (center_x - half, center_y - half, z),
        (center_x + half, center_y - half, z),
        (center_x + half, center_y + half, z),
        (center_x - half, center_y + half, z)])
segments = []
for ring in rings:
    for i in range(4):
        segments.extend([
            ring[i],
            ring[(i + 1) % 4]])
for lower, upper in zip(rings[:-1], rings[1:]):
    for i in range(4):
        segments.extend([
            lower[i],
            upper[i]])
        segments.extend([
            lower[i],
            upper[(i + 1) % 4]])
self.create_lines(segments, (17, 17, 17, 235), width = 2)
