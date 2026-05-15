# Source Generated with Decompyle++
# File: 057_EdgeDiagnosticsApp.crew_action_circulate.pyc (Python 3.12)

self.log_crew('Вы: Промывка скважины!')
self.hole_cleaning = min(100, self.hole_cleaning + 25)
self.log_crew(f'''Бурильщик: Промывка, очистка ствола: {self.hole_cleaning:.0f}%.''')
