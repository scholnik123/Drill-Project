# Source Generated with Decompyle++
# File: 035_EdgeDiagnosticsApp.create_lines.pyc (Python 3.12)

if not points:
    return None
item = gl.GLLinePlotItem(pos = np.array(points, dtype = float), color = self._gl_color(color), width = width, antialias = True, mode = mode)
self.view_3d_widget.addItem(item)
self.env_items.append(item)
return item
