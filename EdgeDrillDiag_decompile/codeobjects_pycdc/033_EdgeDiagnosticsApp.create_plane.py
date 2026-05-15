# Source Generated with Decompyle++
# File: 033_EdgeDiagnosticsApp.create_plane.pyc (Python 3.12)

verts = np.array([
    [
        0,
        0,
        0],
    [
        dx,
        0,
        0],
    [
        dx,
        dy,
        0],
    [
        0,
        dy,
        0]])
faces = np.array([
    [
        0,
        1,
        2],
    [
        0,
        2,
        3]])
c = self._gl_color(color)
colors = np.array([
    c] * 2)
mesh = gl.GLMeshItem(vertexes = verts, faces = faces, faceColors = colors, smooth = False)
mesh.translate(px, py, pz)
self.view_3d_widget.addItem(mesh)
self.env_items.append(mesh)
return mesh
