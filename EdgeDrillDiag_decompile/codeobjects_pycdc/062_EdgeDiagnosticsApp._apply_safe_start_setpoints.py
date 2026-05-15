# Source Generated with Decompyle++
# File: 062_EdgeDiagnosticsApp._apply_safe_start_setpoints.pyc (Python 3.12)

D = max(10, float(self.conf_depth))
pore_grad = max(0.85, float(self.geo_pore_grad))
frac_grad = max(pore_grad + 0.12, float(self.geo_frac_grad))
flow = float(np.interp(D, [
    10,
    500,
    1500,
    3000,
    4000,
    5500], [
    900,
    1050,
    1250,
    1400,
    1500,
    1600]))
if self.conf_profile in ('DIRECTIONAL', 'S-SHAPE', 'J-SHAPE'):
    flow += 100
elif self.conf_profile in ('HORIZONTAL', 'MULTILATERAL'):
    flow += 180
if self.conf_rig in ('OFFSHORE', 'SEMI-SUB', 'DRILLSHIP'):
    flow += 80
flow = max(850, min(2200, flow))
rpm = float(np.interp(D, [
    10,
    1500,
    3000,
    5500], [
    80,
    95,
    105,
    115]))
if self.geo_hard > 7:
    rpm += 10
if self.conf_profile in ('HORIZONTAL', 'MULTILATERAL'):
    rpm += 10
rpm = max(70, min(150, rpm))
wob_base = float(np.interp(D, [
    10,
    500,
    1500,
    3000,
    5500], [
    3,
    5,
    8,
    7,
    6]))
if self.geo_hard > 7:
    wob_base -= 1.5
if self.conf_profile != 'VERTICAL':
    wob_base *= 0.85
temp_mult = 1 + max(0, self.act_temp_bh - 120) * 0.005
torque_per_wob = 0.8 + self.geo_hard / 2
if self.conf_profile != 'VERTICAL':
    torque_per_wob += 0.15 * (D / 1000)
safe_wob = (self._startup_twist_limit() * 0.68 / temp_mult - rpm * 0.02) / max(1, torque_per_wob)
wob = max(2, min(wob_base, safe_wob))
if D < 300:
    wob = min(wob, 3.5)
ecd_factor = 1 + (flow / 1500) * 0.05097
lower_mw = (pore_grad + 0.03) / ecd_factor
upper_mw = (frac_grad - 0.08) / ecd_factor
if lower_mw <= upper_mw:
    mudwt = lower_mw + min(0.1, max(0, upper_mw - lower_mw) * 0.45)
else:
    mudwt = (pore_grad + frac_grad) * 0.5 / ecd_factor
mudwt = max(0.9, min(2.35, mudwt))
self.set_rpm = round(rpm, 0)
self.set_wob = round(wob, 1)
self.set_flow = round(flow, 0)
self.set_mudwt = round(mudwt, 2)
self.set_choke = 100
self.act_rpm = self.set_rpm
self.act_wob = self.set_wob
self.act_flow_in = self.set_flow
self.act_flow_out = self.set_flow
self.act_mudwt = self.set_mudwt
self.act_choke = self.set_choke
AF = (self.act_flow_in / 1500) * (D / 1000) * self.act_mudwt * 0.5
CP = ((100 - self.act_choke) / 100) * (self.act_flow_in / 1000) * self._startup_choke_mult()
self.act_ecd = self.act_mudwt + (AF + CP) / (D * 0.00981) if D > 10 else self.act_mudwt
self.bhp = D * self.act_mudwt * 0.00981 + AF + CP
self.pore_press = D * pore_grad * 0.00981
self.frac_press = D * frac_grad * 0.00981
self.act_spp = (self.act_flow_in / 1000) ** 2 * self.act_mudwt * (D / 1000) * 5 + AF + CP
self.act_torque = (self.act_wob * 0.8 + self.act_rpm * 0.02 + self.act_wob * (self.geo_hard / 2)) * temp_mult
self.log_crew(f'''Автонастройка: MW {self.set_mudwt:.2f} SG, FLOW {self.set_flow:.0f}, WOB {self.set_wob:.1f}, RPM {self.set_rpm:.0f}.''')
