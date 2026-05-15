# Source Generated with Decompyle++
# File: 063_EdgeDiagnosticsApp.reset_sim.pyc (Python 3.12)

self.crew_log.clear()
self.log_crew(f'''Супервайзер: Смена началась. {self.conf_rig}. Профиль: {self.conf_profile}.''')
self.log_crew(f'''Регион: {self.conf_scenario}. Бригада: {self.conf_crew} чел. Старт: {self.conf_depth:.0f}м.''')
self.failure_state = None
self.game_over = False
self.timer_ticks = 0
self.startup_grace_ticks = 120
self.crew_fatigue = 0
self.crew_morale = 100
self.bit_wear = 0
self.hole_cleaning = 100
self.traj_x = 0
self.traj_y = 0
self.traj_z = 0
self.trajectory_points = [
    (0, 0, 0)]
self.act_vibration = 0
self.act_drag = 0
self.act_inclination = 0
self.act_azimuth = random.uniform(0, 360)
self.set_rpm = 100
self.set_wob = 15
self.set_flow = 1500
self.set_mudwt = 1.1
self.set_choke = 100
self.act_rpm = 100
self.act_torque = 11
self.act_wob = 15
self.act_rop = 20
self.act_spp = 15
self.act_flow_in = 1500
self.act_flow_out = 1500
self.act_pit = 50
self.act_mudwt = 1.1
self.act_choke = 100
self.depth = self.conf_depth
import numpy as np
self.trajectory_points = []
for d in range(0, int(self.conf_depth) + 1, 50):
    self.trajectory_points.append((0, float(d), 0))
if not self.trajectory_points:
    self.trajectory_points.append((0, 0, 0))
self.traj_x = 0
self.traj_y = float(self.conf_depth)
self.traj_z = 0
self.update_geology(force = True)
self._apply_safe_start_setpoints()
self._build_3d_environment()
if hasattr(self, 'traj_line'):
    self._sync_3d_trajectory(follow = False)
    self._fit_3d_well()
self.l_rpm.setText(f'''RPM: {self.set_rpm:.0f}''')
self.l_wob.setText(f'''WOB: {self.set_wob:.1f}''')
self.l_flw.setText(f'''FLOW: {self.set_flow:.0f}''')
self.l_mw.setText(f'''MUDWT: {self.set_mudwt:.2f}''')
self.l_chk.setText(f'''CHOKE: {self.set_choke:.0f}%''')
self.lbl_diff.setText(f'''[{self.conf_rig}] | [{self.conf_profile}] | {self.conf_scenario} | DIFF: {self.conf_diff}''')
self.btn_reset.hide()
