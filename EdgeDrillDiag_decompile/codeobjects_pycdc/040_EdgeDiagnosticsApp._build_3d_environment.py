# Source Generated with Decompyle++
# File: 040_EdgeDiagnosticsApp._build_3d_environment.pyc (Python 3.12)

if not hasattr(self, 'env_items'):
    self.env_items = []
for item in self.env_items:
    self.view_3d_widget.removeItem(item)
self.env_items.clear()
self.create_depth_scale()
self.create_geology_layers()
is_offshore = self.conf_rig in ('OFFSHORE', 'SEMI-SUB', 'DRILLSHIP')
if not is_offshore:
    self.create_plane(4000, 4000, (60, 100, 60, 255), -2000, -2000, -10)
    self.create_plane(4000, 40, (50, 50, 50, 255), -2000, -120, -9)
    self.create_box(80, 20, 5, (80, 80, 80, 255), -10, -50, -10)
    for i in range(5):
        self.create_cylinder(1.5, 75, (200, 200, 200, 255), -5, -45 + i * 3, -5, rot_y = 90)
    for i in range(4):
        self.create_box(70, 2, 2, (35, 35, 35, 255), -5, -55 + i * 7, 0)
    for i in range(3):
        self.create_box(15, 30, 10, (100, 100, 150, 255), -60, -30 + i * 35, -10)
        self.create_box(12, 24, 2, (70, 70, 120, 255), -58.5, -27 + i * 35, 1)
    self.create_box(40, 20, 15, (150, 150, 150, 255), -50, -80, -10)
    self.create_box(12, 10, 5, (60, 60, 60, 255), -6, -5, -2)
    self.create_box(9, 9, 12, (25, 25, 25, 255), -4.5, -4.5, 4)
    self.create_box(40, 40, 10, (180, 180, 180, 255), -20, -20, -10)
    self.create_derrick_frame(base_half = 18, top_half = 4, height = 82)
    self.create_box(12, 12, 6, (40, 40, 40, 255), -6, -6, 75)
    self.create_lines([
        (-24, -24, 8),
        (24, -24, 8),
        (-24, 24, 8),
        (24, 24, 8)], (208, 2, 27, 220), width = 2)
    rng = np.random.default_rng(12)
    for _ in range(36):
        tx = rng.uniform(-1050, 1050)
        ty = rng.uniform(-1050, 1050)
        if abs(tx) < 150 and abs(ty) < 150:
            continue
        self.create_low_poly_tree(tx, ty, rng.uniform(10, 18))
    return None
self.create_plane(5000, 5000, (40, 80, 150, 220), -2500, -2500, -50)
if self.conf_rig == 'DRILLSHIP':
    self.create_box(120, 300, 40, (180, 60, 60, 255), -60, -150, -45)
    self.create_box(100, 40, 30, (200, 200, 200, 255), -50, 80, -5)
    self.create_box(80, 20, 20, (150, 150, 150, 255), -40, 90, 25)
    self.create_box(10, 10, 10, (255, 255, 255, 255), -5, 95, 45)
    self.create_cylinder(40, 2, (50, 150, 50, 255), 0, 150, 25)
    self.create_lines([
        (-42, 132, 27),
        (42, 132, 27),
        (42, 168, 27),
        (-42, 168, 27)], (230, 230, 230, 220), width = 2)
    self.create_box(20, 20, 10, (180, 180, 180, 255), -10, -10, -5)
    self.create_derrick_frame(base_half = 14, top_half = 3, height = 80, z0 = 5)
    self.create_box(12, 12, 6, (40, 40, 40, 255), -6, -6, 80)
    return None
for lx, ly in ((-40, -50), (40, -50), (40, 50), (-40, 50)):
    self.create_cylinder(10, 150, (180, 180, 180, 255), lx, ly, -150)
if self.conf_rig == 'SEMI-SUB':
    for px in (-45, 25):
        self.create_box(20, 140, 20, (100, 100, 100, 255), px, -70, -150)
self.create_box(120, 140, 10, (120, 120, 120, 255), -60, -70, -10)
self.create_box(50, 40, 40, (200, 200, 200, 255), 10, 30, 0)
self.create_cylinder(30, 2, (50, 150, 50, 255), 35, 50, 40)
self.create_lines([
    (10, 20, 44),
    (62, 20, 44),
    (62, 80, 44),
    (10, 80, 44)], (230, 230, 230, 220), width = 2)
self.create_box(30, 30, 15, (150, 150, 150, 255), -40, -40, 0)
self.create_derrick_frame(center_x = -25, center_y = -25, base_half = 18, top_half = 4, height = 88, z0 = 15)
self.create_box(16, 16, 8, (40, 40, 40, 255), -33, -33, 100)
return None
# WARNING: Decompyle incomplete
