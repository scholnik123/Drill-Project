# Source Generated with Decompyle++
# File: 048_EdgeDiagnosticsApp._sync_3d_trajectory.pyc (Python 3.12)

if not hasattr(self, 'traj_line'):
    return None
pos_arr = self._trajectory_scene_points()
if len(pos_arr) == 0:
    return None
self.wellbore_line.setData(pos = pos_arr)
self.traj_line.setData(pos = pos_arr)
self.depth_tick_line.setData(pos = self._trajectory_depth_ticks(pos_arr))
self.drill_bit.setData(pos = np.array([
    pos_arr[-1]], dtype = float))
if follow:
    if self.camera_follow_drill:
        self.view_3d_widget.opts['center'] = pg.Vector(pos_arr[-1][0], pos_arr[-1][1], pos_arr[-1][2])
        return None
    return None
