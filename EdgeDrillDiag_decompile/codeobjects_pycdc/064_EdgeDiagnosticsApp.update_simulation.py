# Source Generated with Decompyle++
# File: 064_EdgeDiagnosticsApp.update_simulation.pyc (Python 3.12)

if self.game_over:
    return None
dt = 0.05
if self.conf_diff == 'EASY':
    200 = self, self.timer_ticks += 1, .timer_ticks
    prob_fail = 0
    choke_mult = 5
    twist_limit = 60
elif self.conf_diff == 'NORMAL':
    geo_change_freq = 100
    prob_fail = 0.02
    choke_mult = 10
    twist_limit = 45
else:
    geo_change_freq = 60
    prob_fail = 0.08
    choke_mult = 20
    twist_limit = 35
if self.timer_ticks % geo_change_freq == 0:
    self.update_geology()
self.crew_fatigue = min(100, self.crew_fatigue + 0.002 * (8 - self.conf_crew))
if self.crew_fatigue > 70:
    self.crew_morale = max(0, self.crew_morale - 0.01)
if self.crew_fatigue > 90 and self.timer_ticks % 500 == 0:
    self.log_crew('⚠ Бригада устала! Риск ошибок!')
self.bit_wear = min(100, self.bit_wear + self.geo_hard * self.act_wob * self.act_rpm * 2e-06)
bit_eff = max(0.1, 1 - self.bit_wear / 120)
if self.bit_wear > 80 and self.timer_ticks % 300 == 0:
    self.log_crew(f'''Механик: Износ долота {self.bit_wear:.0f}%! Рекомендую подъём!''')
clean_factor = (self.act_flow_in / 1500) * (self.act_rpm / 100)
if self.conf_profile in ('HORIZONTAL', 'MULTILATERAL'):
    clean_factor *= 0.6
elif self.conf_profile in ('DIRECTIONAL', 'S-SHAPE', 'J-SHAPE'):
    clean_factor *= 0.8
max(0, min(100, self.hole_cleaning)) = self, self.hole_cleaning += (clean_factor * 100 - self.hole_cleaning) * 0.01, .hole_cleaning
if np.random.random() < 0.005:
    msgs = [
        'Бурильщик: Вибрации в норме.',
        'Растворщик: Запас барита на исходе.',
        'Механик: Подшипники ВСП в норме.',
        'Деррикман: Емкости стабильны.',
        f'''Геолог: Шлам — {self.geo_name.split()[0].lower()}.''',
        f'''Телеметрист: Забой {self.act_temp_bh:.0f}°C.''']
    if self.conf_profile != 'VERTICAL':
        msgs.append(f'''Телеметрист: Зенит {self.act_inclination:.1f}°, азимут {self.act_azimuth:.0f}°.''')
    self.log_crew(random.choice(msgs))
if self.conf_profile == 'VERTICAL':
    tgt_incl = 0
elif self.conf_profile == 'DIRECTIONAL':
    tgt_incl = min(45, max(0, (self.depth - self.conf_depth) * 0.15))
elif self.conf_profile == 'HORIZONTAL':
    bs = self.conf_depth + 50
    tgt_incl = 0 if self.depth < bs else min(90, (self.depth - bs) * 0.2)
elif self.conf_profile == 'S-SHAPE':
    seg = self.depth - self.conf_depth
    if seg < 200:
        tgt_incl = min(35, seg * 0.2)
    elif seg < 400:
        tgt_incl = max(0, 35 - (seg - 200) * 0.2)
    else:
        tgt_incl = 0
elif self.conf_profile == 'J-SHAPE':
    tgt_incl = min(60, max(0, (self.depth - self.conf_depth) * 0.1))
elif self.conf_profile == 'MULTILATERAL':
    tgt_incl = min(75, max(0, (self.depth - self.conf_depth) * 0.15))
else:
    tgt_incl = 0
if self.conf_profile != 'VERTICAL':
    pass
0 = self, self.act_azimuth += np.random.normal(0, 0.1), .act_azimuth
if self.conf_rig in ('OFFSHORE', 'SEMI-SUB', 'DRILLSHIP'):
    if self.conf_rig == 'DRILLSHIP':
        pass
    elif self.conf_rig == 'SEMI-SUB':
        pass
    
    heave = 1.5 * 1
    wob_noise = np.sin(self.timer_ticks * 0.2) * heave
resp = max(0.5, 1 - self.crew_fatigue / 200)
if self.failure_state != 'PUMP_FAILURE':
    pass
max(10, self.depth) = self, self.act_choke += (self.set_choke - self.act_choke) * 0.2, .act_choke
MW = self.act_mudwt
HP = D * MW * 0.00981
AF = (self.act_flow_in / 1500) * (D / 1000) * MW * 0.5
CP = ((100 - self.act_choke) / 100) * (self.act_flow_in / 1000) * choke_mult
self.act_ecd = MW + (AF + CP) / (D * 0.00981) if D > 10 else MW
BHP = HP + AF + CP
PP = D * self.geo_pore_grad * 0.00981
FP = D * self.geo_frac_grad * 0.00981
tgt_spp = (self.act_flow_in / 1000) ** 2 * MW * (D / 1000) * 5 + AF + CP
tgt_rop = 0
tgt_torque = self.act_wob * 0.8 + self.act_rpm * 0.02
incl_rad = np.radians(self.act_inclination)
drag_force = self.act_wob * np.sin(incl_rad) * 0.25 * (D / 1000)
self.act_drag = drag_force
if self.conf_profile != 'VERTICAL':
    tgt_torque += drag_force * 0.5 + (self.act_inclination / 10) * (D / 1000)
if self.act_temp_bh > 120:
    tgt_torque *= 1 + (self.act_temp_bh - 120) * 0.005
if self.act_flow_in > 100:
    tgt_rop = self.act_wob * self.act_rpm * bit_eff / (self.geo_hard * 40)
    if self.hole_cleaning < 60:
        tgt_rop *= self.hole_cleaning / 60
    tgt_torque += self.act_wob * (self.geo_hard / 2)
vib_base = abs(self.act_wob - self.act_rpm * 0.1) * 0.3
if self.geo_hard > 7:
    vib_base *= 1.5
if self.conf_profile in ('HORIZONTAL', 'S-SHAPE'):
    vib_base *= 1.3
max(0, self.act_vibration) = self, self.act_vibration += (vib_base - self.act_vibration) * 0.1 + np.random.normal(0, 0.3), .act_vibration
tgt_flow_out = self.act_flow_in
tgt_pit_delta = 0
ai_msg = f'''СИСТЕМЫ В НОРМЕ. ECD: {self.act_ecd:.2f} SG. ОЧИСТКА: {self.hole_cleaning:.0f}%.'''
ai_class = 'NormalBox'
can_fail = self.timer_ticks > getattr(self, 'startup_grace_ticks', 0)
pump_limit = 35 + (D / 1000) * 24 + max(0, MW - 1) * 25
if can_fail and self.act_torque > twist_limit and self.failure_state != 'TWIST_OFF':
    self.failure_state = 'TWIST_OFF'
    self.log_crew('🚨 ОБРЫВ БУРИЛЬНОЙ КОЛОННЫ!')
stuck_prob = prob_fail * 2 if self.conf_profile != 'VERTICAL' else 1
# WARNING: Decompyle incomplete
