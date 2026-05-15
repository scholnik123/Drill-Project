# Source Generated with Decompyle++
# File: 032_EdgeDiagnosticsApp.create_box.pyc (Python 3.12)

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
        0],
    [
        0,
        0,
        dz],
    [
        dx,
        0,
        dz],
    [
        dx,
        dy,
        dz],
    [
        0,
        dy,
        dz]])
faces = np.array([
    [
        0,
        1,
        2],
    [
        0,
        2,
        3],
    [
        4,
        5,
        6],
    [
        4,
        6,
        7],
    [
        0,
        1,
        5],
    [
        0,
        5,
        4],
    [
        2,
        3,
        7],
    [
        2,
        7,
        6],
    [
        1,
        2,
        6],
    [
        1,
        6,
        5],
    [
        0,
        3,
        7],
    [
        0,
        7,
        4]])
c = self._gl_color(color)
colors = np.array([
    c] * 12)
mesh = gl.GLMeshItem(vertexes = verts, faces = faces, faceColors = colors, smooth = False, drawEdges = True, edgeColor = (0, 0, 0, 1))
mesh.translate(px, py, pz)
self.view_3d_widget.addItem(mesh)
self.env_items.append(mesh)
return mesh
