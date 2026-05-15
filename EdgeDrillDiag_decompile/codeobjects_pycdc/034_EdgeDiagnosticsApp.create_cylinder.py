# Source Generated with Decompyle++
# File: 034_EdgeDiagnosticsApp.create_cylinder.pyc (Python 3.12)

md = gl.MeshData.cylinder(rows = 2, cols = segments, radius = [
    radius,
    radius], length = height)
c = self._gl_color(color)
colors = np.array([
    c] * md.faceCount())
mesh = gl.GLMeshItem(meshdata = md, faceColors = colors, smooth = False, drawEdges = True, edgeColor = (0, 0, 0, 0.55))
mesh.translate(0, 0, -height / 2)
if rot_x:
    mesh.rotate(rot_x, 1, 0, 0)
if rot_y:
    mesh.rotate(rot_y, 0, 1, 0)
if rot_z:
    mesh.rotate(rot_z, 0, 0, 1)
mesh.translate(px, py, pz + height / 2)
self.view_3d_widget.addItem(mesh)
self.env_items.append(mesh)
return mesh
