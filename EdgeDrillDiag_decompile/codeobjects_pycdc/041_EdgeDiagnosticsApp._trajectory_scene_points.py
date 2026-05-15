# Source Generated with Decompyle++
# File: 041_EdgeDiagnosticsApp._trajectory_scene_points.pyc (Python 3.12)

if not self.trajectory_points:
    return np.empty((0, 3), dtype = float)
step = None(1, len(self.trajectory_points) // 500)
pts = self.trajectory_points[::step]
if pts[-1] != self.trajectory_points[-1]:
    pts.append(self.trajectory_points[-1])
# WARNING: Decompyle incomplete
