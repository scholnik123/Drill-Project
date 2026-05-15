# Source Generated with Decompyle++
# File: 051_EdgeDiagnosticsApp.change_setpoint.pyc (Python 3.12)

if self.game_over:
    return None
if param == 'set_rpm':
    self.set_rpm = max(0, min(250, self.set_rpm + delta))
elif param == 'set_wob':
    self.set_wob = max(0, min(50, self.set_wob + delta))
elif param == 'set_flow':
    self.set_flow = max(0, min(3500, self.set_flow + delta))
elif param == 'set_mudwt':
    self.set_mudwt = max(0.8, min(2.5, self.set_mudwt + delta))
elif param == 'set_choke':
    self.set_choke = max(0, min(100, self.set_choke + delta))
lbl.setText(fmt(getattr(self, param)))
