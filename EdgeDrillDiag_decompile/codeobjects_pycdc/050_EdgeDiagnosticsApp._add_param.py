# Source Generated with Decompyle++
# File: 050_EdgeDiagnosticsApp._add_param.pyc (Python 3.12)

vbox = QVBoxLayout()
vbox.setSpacing(0)
lbl_n = QLabel(name)
lbl_n.setProperty('class', 'ParamName')
lbl_v = QLabel('0.0')
lbl_v.setProperty('class', 'ParamVal')
vbox.addWidget(lbl_n)
vbox.addWidget(lbl_v)
grid.addLayout(vbox, r, c)
return lbl_v
