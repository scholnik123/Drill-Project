# Source Generated with Decompyle++
# File: 011_EdgeDiagnosticsApp._finish_window_geometry_guard_later.pyc (Python 3.12)

if self._window_geometry_guard:
    QTimer.singleShot(0, self._finish_window_geometry_guard)
    return None
