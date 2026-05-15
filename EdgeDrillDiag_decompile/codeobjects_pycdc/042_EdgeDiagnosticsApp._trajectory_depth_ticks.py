# Source Generated with Decompyle++
# File: 042_EdgeDiagnosticsApp._trajectory_depth_ticks.pyc (Python 3.12)

if len(pos_arr) == 0:
    return np.empty((0, 3), dtype = float)
stride = None(1, len(pos_arr) // 28)
segments = []
for p in pos_arr[::stride]:
    span = 12
    segments.extend([
        (p[0] - span, p[1], p[2]),
        (p[0] + span, p[1], p[2])])
    segments.extend([
        (p[0], p[1] - span, p[2]),
        (p[0], p[1] + span, p[2])])
return np.array(segments, dtype = float)
