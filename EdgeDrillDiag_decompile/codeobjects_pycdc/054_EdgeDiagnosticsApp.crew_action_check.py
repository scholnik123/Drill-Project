# Source Generated with Decompyle++
# File: 054_EdgeDiagnosticsApp.crew_action_check.pyc (Python 3.12)

self.log_crew('Вы: Верховой, проверь крепления!')
if self.act_vibration > 5:
    self.log_crew('Верховой: Есть люфт в подвеске!')
else:
    self.log_crew('Верховой: Всё в норме, крепления затянуты.')
