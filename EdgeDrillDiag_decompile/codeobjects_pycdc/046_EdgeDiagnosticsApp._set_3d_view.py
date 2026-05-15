# Source Generated with Decompyle++
# File: 046_EdgeDiagnosticsApp._set_3d_view.pyc (Python 3.12)

self._disable_3d_follow()
(center, distance) = self._well_camera_target()
self.view_3d_widget.opts['center'] = center
self.view_3d_widget.setCameraPosition(distance = distance, elevation = elevation, azimuth = azimuth)
