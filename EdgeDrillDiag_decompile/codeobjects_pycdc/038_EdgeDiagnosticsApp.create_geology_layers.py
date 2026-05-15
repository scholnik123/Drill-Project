# Source Generated with Decompyle++
# File: 038_EdgeDiagnosticsApp.create_geology_layers.pyc (Python 3.12)

colors = [
    (156, 120, 70, 55),
    (120, 110, 92, 48),
    (190, 178, 128, 45),
    (96, 118, 132, 45),
    (150, 95, 82, 42),
    (92, 92, 92, 45)]
for idx, depth in enumerate(range(500, 6001, 500)):
    size = 1700 - (idx % 4) * 70
    self.create_plane(size, size, colors[idx % len(colors)], -size / 2, -size / 2, -depth)
