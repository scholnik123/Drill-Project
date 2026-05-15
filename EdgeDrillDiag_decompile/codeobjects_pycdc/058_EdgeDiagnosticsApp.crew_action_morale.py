# Source Generated with Decompyle++
# File: 058_EdgeDiagnosticsApp.crew_action_morale.pyc (Python 3.12)

self.crew_morale = min(100, self.crew_morale + 15)
self.crew_fatigue = max(0, self.crew_fatigue - 5)
msgs = [
    'Вы: Отличная работа, бригада!',
    'Вы: Чай/кофе за мой счёт!',
    'Вы: Молодцы, темп хороший!']
self.log_crew(random.choice(msgs))
self.log_crew('Бригада: Спасибо, шеф! 💪')
