# Source Generated with Decompyle++
# File: 017_EdgeDiagnosticsApp.apply_config_and_start.pyc (Python 3.12)

rig = self.cb_rig.currentText()
if 'ONSHORE' in rig:
    self.conf_rig = 'ONSHORE'
elif 'SEMI' in rig:
    self.conf_rig = 'SEMI-SUB'
elif 'DRILLSHIP' in rig:
    self.conf_rig = 'DRILLSHIP'
else:
    self.conf_rig = 'OFFSHORE'
prof = self.cb_profile.currentText()
for key in ('VERTICAL', 'DIRECTIONAL', 'HORIZONTAL', 'S-SHAPE', 'J-SHAPE', 'MULTILATERAL'):
    if not key in prof:
        continue
    self.conf_profile = key
    ('VERTICAL', 'DIRECTIONAL', 'HORIZONTAL', 'S-SHAPE', 'J-SHAPE', 'MULTILATERAL')
depths = [
    0,
    500,
    1500,
    3000,
    4000,
    5500]
self.conf_depth = depths[min(self.cb_depth.currentIndex(), len(depths) - 1)]
diff = self.cb_diff.currentText()
if 'ТРЕНИРОВКА' in diff:
    self.conf_diff = 'EASY'
elif 'СТАНДАРТ' in diff:
    self.conf_diff = 'NORMAL'
else:
    self.conf_diff = 'HARD'
self.conf_scenario = self.cb_scenario.currentText()
self.conf_crew = int(self.cb_crew.currentText()[0])
self._begin_window_geometry_guard()
self.stacked.setCurrentIndex(1)
self._finish_window_geometry_guard_later()
self.reset_sim()
self.timer.start(50)
