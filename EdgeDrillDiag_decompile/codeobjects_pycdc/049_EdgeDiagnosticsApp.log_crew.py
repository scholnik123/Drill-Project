# Source Generated with Decompyle++
# File: 049_EdgeDiagnosticsApp.log_crew.pyc (Python 3.12)

time_str = time.strftime('%H:%M:%S')
self.crew_log.insertItem(0, f'''[{time_str}] {msg}''')
if self.crew_log.count() > 50:
    self.crew_log.takeItem(self.crew_log.count() - 1)
    return None
