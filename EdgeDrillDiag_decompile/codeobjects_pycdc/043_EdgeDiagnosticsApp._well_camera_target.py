# Source Generated with Decompyle++
# File: 043_EdgeDiagnosticsApp._well_camera_target.pyc (Python 3.12)

pos_arr = self._trajectory_scene_points()
if len(pos_arr) == 0:
    center = pg.Vector(0, 0, -max(0, float(self.depth)) * 0.5)
    return (center, 900)
mins = None.min(axis = 0)
maxs = pos_arr.max(axis = 0)
None[None, mins, None, 2, mins[:2] -= 180, , :] = None
None[None, maxs, None, 2, maxs[:2] += 180, , :] = None
np.maximum(maxs - mins, 1) = (mins + maxs) * 0.5
distance = max(650, float(max(span[2] * 1.28, span[0] * 2.2, span[1] * 2.2)))
distance = min(self.view_3d_widget.max_zoom_distance, distance)
return (pg.Vector(center_arr[0], center_arr[1], center_arr[2]), distance)
