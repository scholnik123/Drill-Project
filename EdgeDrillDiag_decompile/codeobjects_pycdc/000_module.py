# Source Generated with Decompyle++
# File: 000_module.pyc (Python 3.12)

import sys
import random
import time
import numpy as np
import pyqtgraph as pg
from pyqtgraph.opengl import opengl as gl
from PySide6.QtWidgets import QApplication, QMainWindow, QWidget, QVBoxLayout, QHBoxLayout, QLabel, QPushButton, QFrame, QGridLayout, QProgressBar, QStackedWidget, QListWidget, QComboBox, QTabWidget
from PySide6.QtCore import QTimer, Qt
STYLESHEET = "\nQMainWindow { background-color: #EFECE6; }\nQLabel { color: #111111; font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; }\n.Panel { background-color: #EFECE6; border: 2px solid #111111; border-radius: 0px; }\n.Title { font-size: 14px; font-weight: 900; color: #111111; letter-spacing: 2px; text-transform: uppercase; border-bottom: 2px solid #111111; padding-bottom: 5px; margin-bottom: 5px;}\n.ParamName { font-size: 10px; font-weight: 800; color: #555555; text-transform: uppercase; letter-spacing: 1px;}\n.ParamVal { font-size: 26px; font-weight: 900; color: #000000; letter-spacing: -1px;}\n.AlertBox { background-color: #D0021B; border: 2px solid #111111; padding: 15px; color: #FFFFFF; font-weight: 900; font-size: 14px; text-transform: uppercase;}\n.NormalBox { background-color: #EFECE6; border: 2px solid #111111; padding: 15px; color: #111111; font-weight: 900; font-size: 14px; text-transform: uppercase;}\n.InfoBox { background-color: #EFECE6; border: 2px solid #111111; padding: 8px; color: #111111; font-weight: 800; font-size: 12px; text-transform: uppercase;}\nQPushButton { background-color: #EFECE6; color: #111111; border: 2px solid #111111; padding: 10px; font-size: 12px; font-weight: 900; border-radius: 0px; text-transform: uppercase; letter-spacing: 1px;}\nQPushButton:hover { background-color: #111111; color: #EFECE6; }\nQPushButton.Danger { background-color: #111111; color: #EFECE6; border: 2px solid #111111; }\nQPushButton.Danger:hover { background-color: #D0021B; color: #FFFFFF; border-color: #D0021B;}\nQProgressBar { border: 2px solid #111111; background: #EFECE6; text-align: center; color: #111111; font-size: 12px; font-weight: 900; border-radius: 0px; text-transform: uppercase; letter-spacing: 2px;}\nQProgressBar::chunk { background-color: #111111; }\nQListWidget { background-color: #EFECE6; color: #111111; border: 2px solid #111111; font-family: 'Courier New', Courier, monospace; font-size: 12px; font-weight: bold;}\nQListWidget::item { border-bottom: 1px dashed #AAAAAA; padding: 4px; }\nQListWidget::item:selected { background-color: #111111; color: #EFECE6; }\nQListWidget::item:hover { background-color: #D5D2CC; color: #111111; }\nQComboBox { background-color: #EFECE6; color: #111111; border: 2px solid #111111; padding: 5px; font-weight: bold; font-size: 12px; }\nQComboBox QAbstractItemView { background-color: #EFECE6; color: #111111; selection-background-color: #111111; selection-color: #EFECE6; border: 2px solid #111111; }\nQComboBox QAbstractItemView::item { padding: 5px; }\nQComboBox QAbstractItemView::item:hover { background-color: #D5D2CC; color: #111111; }\nQScrollBar:vertical { background: #EFECE6; width: 12px; border: 1px solid #AAAAAA; }\nQScrollBar::handle:vertical { background: #888888; min-height: 30px; border-radius: 0px; }\nQScrollBar::handle:vertical:hover { background: #111111; }\nQScrollBar::add-line:vertical, QScrollBar::sub-line:vertical { height: 0px; }\nQTabWidget::pane { border: 2px solid #111111; background: #EFECE6; }\nQTabBar::tab { background: #D5D2CC; color: #111111; border: 2px solid #111111; border-bottom: none; padding: 10px 20px; font-weight: bold; font-size: 14px; margin-right: 2px; }\nQTabBar::tab:selected { background: #111111; color: #EFECE6; }\n\n"

class ZoomableGLViewWidget(gl.GLViewWidget):
    pass
# WARNING: Decompyle incomplete


class EdgeDiagnosticsApp(QMainWindow):
    pass
# WARNING: Decompyle incomplete

if __name__ == '__main__':
    app = QApplication(sys.argv)
    window = EdgeDiagnosticsApp()
    window.show()
    sys.exit(app.exec())
    return None
