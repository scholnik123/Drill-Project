# Source Generated with Decompyle++
# File: 055_EdgeDiagnosticsApp.crew_action_trip.pyc (Python 3.12)

if self.bit_wear > 60:
    self.log_crew('Вы: Подъём инструмента для смены долота!')
    self.log_crew(f'''Бурильщик: Подъём с {int(self.depth)}м, ~{int(self.depth / 300)} свечей.''')
    self.bit_wear = 0
    self.log_crew('Механик: Новое долото установлено.')
    return None
self.log_crew(f'''Вы: Износ долота: {self.bit_wear:.0f}%. Смена не требуется.''')
