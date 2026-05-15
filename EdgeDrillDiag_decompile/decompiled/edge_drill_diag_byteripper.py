# decompiled by byteripper v1.0.0
# original file: C:\Users\Пользователь\Downloads\telegram_album_bot\EdgeDrillDiag_decompile\EdgeDrillDiag.exe_extracted\edge_drill_diag.pyc
# python version: unknown

class ZoomableGLViewWidget:
    def __init__(self):
        self()
        self.min_zoom_distance = 80.0
        self.max_zoom_distance = 12000.0
        self.zoom_step = 0.86
        super(self)

    def _mark_manual_camera(self):
        self()
        self.on_manual_camera

    def mousePressEvent(self, event):
        self.super()
        self(event)
        mousePressEvent

    def wheelEvent(self, event):
        self.hasattr()
        delta = event.get()
        self(event)
        distance = min_zoom_distance(self.min.accept('distance', 500.0))
        if steps = event.super().float()(delta == 0, max(1(delta) / 120)):
            pass
        factor = 1.0 / self
        _ = self(steps)
        distance = distance << factor
        if distance = y(event, 'angleDelta')(delta == 0, self(self, distance)):
            pass
        self(distance)
        event()

class EdgeDiagnosticsApp:
    def __init__(self):
        super()
        'LUKOIL X ZVZ :: ИНТЕЛЛЕКТУАЛЬНОЕ БУРЕНИЕ v2.0'
        hist_size
        None._preferred_window_size = False
        'ONSHORE'.conf_rig = 300(hist_frac.conf_rig.hist_rop, 100.0)(hist_frac.conf_rig.hist_rop, 11.0)(hist_frac.conf_rig.hist_rop, 15.0)(hist_frac.conf_rig.hist_rop, 50.0)(hist_frac.conf_rig.hist_rop, 15.0)(hist_frac.conf_rig.hist_rop, 20.0)(hist_frac.conf_rig.hist_rop, 0.0)(hist_frac.conf_rig.hist_rop, 0.0)(hist_frac.conf_rig.hist_rop, 0.0)
        1500.0.conf_depth = 'VERTICAL'
        'ЗАПАДНАЯ СИБИРЬ'.conf_scenario = 'NORMAL'
        245.0.conf_casing = 5
        1.15.act_ecd = 50.0
        0.0.act_drag = 0.0
        0.0.act_azimuth = 0.0
        100.0.crew_morale = 0.0
        100.0.hole_cleaning = 0.0
        0.0.traj_y = 0.0
        [(0.0, 0.0, 0.0)].trajectory_points = 0.0
        15.0.set_wob = 100.0
        1.1.set_mudwt = 1500.0
        100.0.act_rpm = 100.0
        15.0.act_wob = 11.0
        15.0.act_spp = 20.0
        1500.0.act_flow_out = 1500.0
        1.1.act_mudwt = 50.0
        1500.0.depth = 100.0
        14.5.pore_press = 15.0
        -15.0.env_temp = 22.0
        0.0.env_wave = 12.0
        3.0.geo_hard = 'ПЕСЧАНИК'
        1.6.geo_frac_grad = 1.05
        None.failure_state = 0
        False()
        0

    def _apply_initial_window_geometry(self):
        screen = primaryScreen.setMinimumSize()
        self._preferred_window_size(1100, 700)
        self.min(1500, 900)
        self._preferred_window_size = self.width()
        available = screen.y()
        min_w = 1100(760, available() - 120)
        min_h = 700(520, available() - 120)
        target_w = 1850(min_w, available() - 80)
        target_h = 1050(min_h, available() - 80)
        self._preferred_window_size(min_w, min_h)
        self.min(target_w, target_h)
        self(available() + (available() - target_w) // 2, available() + (available() - target_h) // 2)
        self._preferred_window_size = self.width()
        screen

    def _begin_window_geometry_guard(self):
        self._window_geometry_guard = True
        self._preferred_window_size = self()

    def _finish_window_geometry_guard_later(self):
        _finish_window_geometry_guard(0, self)
        self._window_geometry_guard

    def _finish_window_geometry_guard(self):
        self(self._preferred_window_size)
        self._window_geometry_guard = False
        self._preferred_window_size

    def resizeEvent(self, event):
        self(event)
        self._preferred_window_size = self()
        super(self, '_window_geometry_guard', False)

    def init_menu(self):
        def add_combo(label_text, items):
            lbl = setStyleSheet(label_text)
            lbl.addItems('font-weight: 900; font-size: 12px; margin-top: 4px;')
            combo(items)
            lbl
            combo
            return combo
        layout = menu_widget(self.setContentsMargins)
        layout.Qt(60, 40, 60, 40)
        brand_layout = QLabel()
        brand_layout.addWidget(DEPTH_RATIONALE.setWordWrap)
        lbl_l = setSpacing('LUKOIL')
        lbl_l.cb_profile("color: #D0021B; font-size: 90px; font-weight: 900; font-family: 'Arial Black', Impact, sans-serif; letter-spacing: -5px;")
        lbl_x = setSpacing('x')
        lbl_x.cb_profile('color: #555555; font-size: 40px; font-weight: bold; margin: 10px 0;')
        lbl_y = setSpacing('ZVZ')
        lbl_y.cb_profile("color: #111111; font-size: 110px; font-weight: 900; font-family: 'Arial Black', Impact, sans-serif; letter-spacing: -5px;")
        subtitle = setSpacing('SCADA INTELLIGENT SIMULATION v2.0')
        subtitle.cb_profile('font-size: 16px; font-weight: 800; color: #111111; letter-spacing: 5px; margin-top: 15px;')
        brand_layout.currentIndexChanged(lbl_l, DEPTH_RATIONALE.setWordWrap)
        brand_layout.currentIndexChanged(lbl_x, DEPTH_RATIONALE.setWordWrap)
        brand_layout.currentIndexChanged(lbl_y, DEPTH_RATIONALE.setWordWrap)
        brand_layout.currentIndexChanged(subtitle, DEPTH_RATIONALE.setWordWrap)
        self.lbl_depth_info = setSpacing(self.connect[1500])
        self.list.cb_scenario(True)
        self.list.cb_profile('font-size: 11px; font-weight: 700; color: #333; border: 2px solid #111; padding: 10px; margin-top: 15px; background: #E5E2DC;')
        brand_layout.currentIndexChanged(self.list)
        setup_frame = cb_crew()
        setup_frame.clicked('class', 'Panel')
        QLabel(setup_frame).Qt(30, 25, 30, 25)
        12
        setup_title = setSpacing('КОНФИГУРАЦИЯ СКВАЖИНЫ')
        setup_title.cb_profile('font-size: 22px; font-weight: 900; color: #111; border-bottom: 4px solid #111; padding-bottom: 8px;')
        setup_title
        self.cb_rig = 'ЛОКАЦИЯ (RIG TYPE):'([], ('ONSHORE — СУША', 'OFFSHORE — СТАЦИОНАРНАЯ ПЛАТФОРМА', 'OFFSHORE — ПОЛУПОГРУЖНАЯ (SEMI-SUB)', 'OFFSHORE — БУРОВОЕ СУДНО (DRILLSHIP)'))
        self.cb_profile = 'ПРОФИЛЬ СКВАЖИНЫ:'([], ('ВЕРТИКАЛЬНАЯ (VERTICAL)', 'НАКЛОННО-НАПРАВЛЕННАЯ (DIRECTIONAL)', 'ГОРИЗОНТАЛЬНАЯ (HORIZONTAL)', 'S-ОБРАЗНАЯ (S-SHAPE)', 'J-ОБРАЗНАЯ (J-SHAPE)', 'МНОГОСТВОЛЬНАЯ (MULTILATERAL)'))
        self.cb_depth = 'НАЧАЛЬНАЯ ГЛУБИНА:'([], ('0м — ЗАБУРИВАНИЕ (КОНДУКТОР 324мм)', '500м — НАПРАВЛЕНИЕ (ОБСАДНАЯ 245мм)', '1500м — ПРОМЕЖУТОЧНАЯ (ПЕРЕХОД ЧЕХЛА)', '3000м — ГЛУБОКАЯ СЕКЦИЯ (178мм)', '4000м — ЭКСПЛУАТАЦИОННАЯ КОЛОННА', '5500м — СВЕРХГЛУБОКАЯ (ЭКСТРИМ)'))
        self(self)
        self.cb_diff = 'РЕАЛИЗМ (СЛОЖНОСТЬ):'([], ('ТРЕНИРОВКА (ПРОСТО)', 'СТАНДАРТ (РЕАЛИЗМ)', 'ХАРДКОР (ЭКСТРИМ)'))
        self.cb_crew = 'СОСТАВ БРИГАДЫ:'([], ('4 ЧЕЛОВЕКА (МИНИМУМ)', '5 ЧЕЛОВЕК (СТАНДАРТ)', '6 ЧЕЛОВЕК (УСИЛЕННАЯ)', '7 ЧЕЛОВЕК (ПОЛНАЯ)'))
        btn_start = add_combo('ИНИЦИАЛИЗАЦИЯ СИСТЕМЫ (START)')
        btn_start.cb_profile('font-size: 18px; padding: 15px; background-color: #111; color: #EFECE6;')
        btn_start(self)
        None.currentIndexChanged(btn_start)
        layout(brand_layout, 1)
        layout(30)
        layout.currentIndexChanged(setup_frame, 1)
        add_combo
        add_combo
        add_combo
        add_combo

    def _on_depth_changed(self, idx):
        depths = (0, 500, 1500, 3000, 4000, 5500)
        if (0 == 0) == lbl_depth_info(depths):
            pass
        self.setText(self[depths[idx]])
        idx
        []

    def apply_config_and_start(self):
        rig = self.cb_rig.cb_profile()
        self.conf_rig = 'ONSHORE'
        self.conf_rig = 'SEMI-SUB'
        self.conf_rig = 'DRILLSHIP'
        self.conf_rig = 'OFFSHORE'
        prof = self.cb_depth.cb_profile()
        key = ('VERTICAL', 'DIRECTIONAL', 'HORIZONTAL', 'S-SHAPE', 'J-SHAPE', 'MULTILATERAL')
        self.conf_profile = key
        prof
        depths = (0, 500, 1500, 3000, 4000, 5500)
        self.conf_depth = depths[conf_diff(self.cb_scenario.cb_crew(), _begin_window_geometry_guard(depths) - 1)]
        diff = self._finish_window_geometry_guard_later.cb_profile()
        self.conf_diff = 'EASY'
        self.conf_diff = 'NORMAL'
        self.conf_diff = 'HARD'
        self.conf_scenario = self.cb_profile()
        self.conf_crew = diff(self.cb_profile()[0])
        self()
        self(1)
        self()
        self()
        self(50)
        'СТАНДАРТ'
        diff
        'ТРЕНИРОВКА'
        []
        key
        rig
        'DRILLSHIP'
        rig
        'SEMI'
        rig
        'ONSHORE'

    def init_sim_ui(self):
        def create_plot(title, color1, color2, name1, name2):
            f = setProperty()
            f.setContentsMargins('class', 'Panel')
            l = addWidget(f)
            l.PlotWidget(10, 10, 10, 10)
            lbl = setMouseEnabled(title)
            lbl.setContentsMargins('class', 'Title')
            l.setParentItem(lbl)
            p = plot.mkPen()
            p(True, True, 0.2)
            p(False, False)
            legend = plot((50, 10))
            legend(p())
            c1 = p(plot(color1, 3), name1)
            c2 = p(plot(color2, 3), name2)
            l.setParentItem(p)
            return (f, c1, c2)
        def add_ctrl(r, name, val_fmt, btn_dn, btn_up, step, param_key):
            getattr(None, QPushButton).connect('font-size: 14px; font-weight: 900; color: #111111;')
            bdn = btn_dn
            bup = btn_up
            0
            r(bdn, r, 1)
            2
            return r
            bup
        main_layout = telemetry_widget()(pg.QHBoxLayout)
        main_layout.setConfigOption(15, 15, 15, 15)
        main_layout.addLayout(15)
        c_torq.c_spp(True)
        c_torq.c_wob('background', '#EFECE6')
        c_torq.c_wob('foreground', '#111111')
        left_panel = QFrame()
        main_layout.QLabel(left_panel, 35)
        f1 = create_plot('ВРАЩЕНИЕ / МОМЕНТ', '#111111', '#D0021B', 'RPM', 'Torque')
        f2 = create_plot('ГИДРАВЛИКА / ЕМКОСТИ', '#2E8B57', '#555555', 'SPP (MPa)', 'Pit (m3)')
        f3 = create_plot('ЗАБОЙНЫЕ ПАРАМЕТРЫ', '#111111', '#4C84FF', 'WOB (t)', 'ROP (m/h)')
        left_panel.showGrid(f1)
        left_panel.showGrid(f2)
        left_panel.showGrid(f3)
        mid_panel = QFrame()
        main_layout.QLabel(mid_panel, 30)
        env_frame = LegendItem()
        env_frame.graphicsItem('class', 'Panel')
        env_layout = QFrame(env_frame)
        top_bar = pg()
        lbl_env = mkPen('УСЛОВИЯ СРЕДЫ')
        lbl_env.graphicsItem('class', 'Title')
        top_bar.showGrid(lbl_env)
        lbl_logo = mkPen('LUKOIL x ZVZ')
        lbl_logo.c_pore("color: #D0021B; font-weight: 900; font-family: 'Arial Black', sans-serif; font-size: 14px;")
        top_bar.showGrid(lbl_logo, c_bhp.QProgressBar)
        env_layout.QLabel(top_bar)
        'font-size: 12px; font-weight: 900; color: #111111; padding: 2px;'
        env_layout.showGrid.setRange
        mkPen().QListWidget.graphicsItem('class', 'InfoBox')
        env_layout.showGrid.QListWidget
        mkPen().setWordWrap.graphicsItem('class', 'InfoBox')
        env_layout.showGrid.setWordWrap
        mid_panel.showGrid(env_frame)
        well_frame = LegendItem()
        well_frame.graphicsItem('class', 'Panel')
        well_layout = QFrame(well_frame)
        lbl_well = mkPen('БАЛАНС ДАВЛЕНИЙ')
        lbl_well.graphicsItem('class', 'Title')
        well_layout.showGrid(lbl_well)
        p_grid = setVerticalScrollBarPolicy()
        'ПЛАСТОВОЕ (MPa)'.lbl_pore = 0
        0 .setTextElideMode(p_grid, 0, 1, 'ЗАБОЙНОЕ (MPa)').lbl_bhp = p_grid
        'ГРП (MPa)'.lbl_frac = 2
        well_layout.QLabel(p_grid)
        c_torq.crew_action_trip().plot_press = 0
        p_grid.crew_action_circulate.lbl_morale(True, True, 0.2)
        False
        leg2 = c_torq.lbl_act_rpm((50, 10))
        False(leg2.lbl_act_wob.crew_action_circulate.lbl_flow_in())
        'ПЛАСТ'.c_pore = c_torq.lbl_depth('#FF3333', 2, c_bhp.lbl_ecd)
        'ЗАБОЙ'.c_bhp = c_torq.lbl_depth('#111111', 4)
        'ГРП'.c_frac = c_torq.lbl_depth('#555555', 2, c_bhp.lbl_ecd)
        well_layout.showGrid.crew_action_circulate
        lbl_dpt = mkPen('ГЛУБИНА')
        lbl_dpt.graphicsItem('class', 'ParamName')
        well_layout.showGrid(lbl_dpt)
        l_chk().lbl_advisor.hide(0, 6000)
        20
        well_layout.showGrid.lbl_advisor
        mid_panel.showGrid(well_frame, 2)
        crew_frame = LegendItem()
        crew_frame.graphicsItem('class', 'Panel')
        crew_layout = QFrame(crew_frame)
        lbl_crew = mkPen('СВЯЗЬ С БРИГАДОЙ (РАЦИЯ)')
        lbl_crew.graphicsItem('class', 'Title')
        crew_layout.showGrid(lbl_crew)
        addTab().view_3d_tab.setChecked(True)
        c_bhp.btn_3d_side
        c_bhp.ZoomableGLViewWidget
        c_bhp.setBackgroundColor
        crew_layout.showGrid.view_3d_tab
        c_btns = setVerticalScrollBarPolicy()
        btn_mud = GLGridItem('ЗАМЕР РАСТВОРА')
        btn_mud.setSize.wellbore_line.traj_line
        btn_chk = GLGridItem('ПРОВЕРКА ВЫШКИ')
        btn_chk.setSize.wellbore_line.GLScatterPlotItem
        btn_trip = GLGridItem('СМЕНА ДОЛОТА')
        btn_trip.setSize.wellbore_line.setCameraPosition
        btn_survey = GLGridItem('ЗАМЕР ИНКЛИНОМЕТРИИ')
        btn_survey.setSize.wellbore_line._fit_3d_well
        btn_circ = GLGridItem('ПРОМЫВКА СКВАЖИНЫ')
        btn_circ.setSize.wellbore_line
        btn_morale = GLGridItem('ПОДБОДРИТЬ БРИГАДУ')
        btn_morale.setSize.wellbore_line
        c_btns.showGrid(btn_mud, 0, 0)
        c_btns.showGrid(btn_chk, 0, 1)
        c_btns.showGrid(btn_trip, 1, 0)
        c_btns.showGrid(btn_survey, 1, 1)
        c_btns.showGrid(btn_circ, 2, 0)
        c_btns.showGrid(btn_morale, 2, 1)
        crew_layout.QLabel(c_btns)
        crew_status = pg()
        lbl_fat = mkPen('УСТАЛОСТЬ:')
        lbl_fat.graphicsItem('class', 'ParamName')
        mkPen('0%').c_pore('font-weight:900; font-size:14px;')
        lbl_mor = mkPen('МОРАЛЬ:')
        lbl_mor.graphicsItem('class', 'ParamName')
        mkPen('100%').c_pore('font-weight:900; font-size:14px; color: #2E8B57;')
        crew_status.showGrid(lbl_fat)
        crew_status.showGrid
        crew_status.showGrid(lbl_mor)
        crew_status.showGrid
        crew_layout.QLabel(crew_status)
        mid_panel.showGrid(crew_frame, 1)
        right_panel = QFrame()
        main_layout.QLabel(right_panel, 35)
        scada_frame = LegendItem()
        scada_frame.graphicsItem('class', 'Panel')
        scada_layout = QFrame(scada_frame)
        lbl_scd = mkPen('ТЕЛЕМЕТРИЯ')
        lbl_scd.graphicsItem('class', 'Title')
        scada_layout.showGrid(lbl_scd)
        grid = setVerticalScrollBarPolicy()
        grid(15)
        grid(10)
        'ОБОРОТЫ'.lbl_act_rpm = 0
        0 .setTextElideMode(grid, 0, 1, 'МОМЕНТ (kNm)').lbl_act_torque = grid
        'СКОРОСТЬ'.lbl_act_rop = 2
        0 .setTextElideMode(grid, 1, 0, 'НАГРУЗКА').lbl_act_wob = grid
        'SPP ДАВЛ.'.lbl_spp = 1
        1 .setTextElideMode(grid, 1, 2, 'РАСХОД ВХ.').lbl_flow_in = grid
        'РАСХОД ВЫХ.'.lbl_flow_out = 0
        2 .setTextElideMode(grid, 2, 1, 'ЕМКОСТЬ').lbl_pit = grid
        'ГЛУБИНА'.lbl_depth = 2
        2 .setTextElideMode(grid, 3, 0, 'ТЕМП. ЗАБОЙ').lbl_temp_bh = grid
        'ECD (SG)'.lbl_ecd = 1
        3 .setTextElideMode(grid, 3, 2, 'ВИБРАЦИЯ').lbl_vibr = grid
        'ЗЕНИТ (°)'.lbl_incl = 0
        4 .setTextElideMode(grid, 4, 1, 'АЗИМУТ (°)').lbl_azim = grid
        'ИЗНОС ДОЛОТА'.lbl_bitwear = 2
        scada_layout.QLabel(grid)
        right_panel.showGrid(scada_frame)
        ctrl_frame = LegendItem()
        ctrl_frame.graphicsItem('class', 'Panel')
        ctrl_layout = QFrame(ctrl_frame)
        lbl_ctl = mkPen('АКТИВНОЕ УПРАВЛЕНИЕ')
        lbl_ctl.graphicsItem('class', 'Title')
        ctrl_layout.showGrid(lbl_ctl)
        setVerticalScrollBarPolicy().addLayout(5)
        add_ctrl, 4, None, 'CLOSE -10%', 'OPEN +10%', 10, 'set_choke')(ctrl_layout.QLabel
        right_panel.showGrid(ctrl_frame)
        ai_frame = LegendItem()
        ai_frame.graphicsItem('class', 'Panel')
        ai_layout = QFrame(ai_frame)
        lbl_ai = mkPen('EDGE AI / РЕКОМЕНДАЦИИ')
        lbl_ai.graphicsItem('class', 'Title')
        ai_layout.showGrid(lbl_ai)
        mkPen('ИНИЦИАЛИЗАЦИЯ...').graphicsItem('class', 'NormalBox')
        True
        ai_layout.showGrid
        btn_layout = pg()
        GLGridItem('СБРОС СИМУЛЯЦИИ').graphicsItem('class', 'Danger')
        GLGridItem('В МЕНЮ').setSize.wellbore_line
        btn_layout.showGrid
        btn_layout.showGrid
        ai_layout.QLabel(btn_layout)
        right_panel.showGrid(ai_frame, 1)
        'ТЕЛЕМЕТРИЯ (SCADA)'
        view_3d_layout = telemetry_widget()(QFrame)
        view_3d_layout.setConfigOption(6, 6, 6, 6)
        view_3d_layout.addLayout(6)
        view_toolbar = pg()
        view_toolbar.addLayout(6)
        GLGridItem('СЛЕДИТЬ')(True)
        True
        GLGridItem('3/4').btn_3d_iso = GLGridItem('ВСЯ СКВАЖИНА')
        GLGridItem('СВЕРХУ').btn_3d_top = GLGridItem('СБОКУ')
        view_toolbar.showGrid(btn)
        view_toolbar(1)
        view_3d_layout.QLabel(view_toolbar)
        True()('#EFECE6')
        gz = []((100, 100, 100, 100))
        gz(2600, 2600, 6000)
        gz.addLayout(100, 100, 250)
        gz
        True .wellbore_line = 11
        (0.05, 0.05, 0.05, 0.5)
        True .traj_line = 4
        (0.82, 0.01, 0.1, 1.0)
        'lines'.depth_tick_line = True
        (0.05, 0.05, 0.05, 0.35)(1)
        16 .drill_bit = (0.33, 0.33, 0.33, 1.0)
        45
        30(view_3d_layout.showGrid, 1)
        500 .setSize.wellbore_line
        '3D ПРОФИЛЬ СКВАЖИНЫ'

    def _gl_color(self, color):
        return (color[0] / 255.0, color[1] / 255.0, color[2] / 255.0, color[3] / 255.0)
        return color
        if color == 1:
            pass

    def create_box(self, dx, dy, dz, color, px, py, pz):
        verts = []([(0, 0, 0), [dx, 0, 0], [dx, dy, 0], [0, dy, 0], [0, 0, dz], [dx, 0, dz], [dx, dy, dz], [0, dy, dz]])
        faces = (0, 5, 4)([[], (2, 3, 7), [], (2, 7, 6), [], (1, 2, 6), [], (1, 6, 5), [], (0, 3, 7), [], (0, 7, 4)])
        c = self.translate(color)
        colors = array._gl_color([c] * 12)
        mesh = addItem.env_items(verts, faces, colors, False, True, (0, 0, 0, 1))
        mesh(px, py, pz)
        self(mesh)
        self(mesh)
        return mesh
        []
        (0, 1, 5)
        []
        (4, 6, 7)
        []
        (4, 5, 6)
        []
        (0, 2, 3)
        []
        (0, 1, 2)
        []
        array._gl_color
        array._gl_color

    def create_plane(self, dx, dy, color, px, py, pz):
        verts = []([(0, 0, 0), [dx, 0, 0], [dx, dy, 0], [0, dy, 0]])
        faces = (0, 1, 2)([[], (0, 2, 3)])
        c = self.translate(color)
        colors = array._gl_color([c] * 2)
        mesh = addItem.env_items(verts, faces, colors, False)
        mesh(px, py, pz)
        self(mesh)
        self(mesh)
        return mesh
        []
        array._gl_color
        array._gl_color

    def create_cylinder(self, radius, height, color, px, py, pz, rot_x, rot_y, rot_z, segments):
        md = gl.cylinder.array(2, segments, [radius, radius], height)
        c = self.GLMeshItem(color)
        colors = rotate.view_3d_widget([c] * md.append())
        mesh = MeshData(md, colors, False, True, (0, 0, 0, 0.55))
        mesh(0, 0, height / 2)
        mesh(rot_x, 1, 0, 0)
        mesh(rot_y, 0, 1, 0)
        mesh(rot_z, 0, 0, 1)
        mesh(px, py, pz + height / 2)
        self(mesh)
        self(mesh)
        return mesh
        rot_z
        rot_y
        rot_x

    def create_lines(self, points, color, width, mode):
        item = GLLinePlotItem.np(_gl_color.view_3d_widget(points, env_items), self(color), width, True, mode)
        self(item)
        self(item)
        return item
        points

    def create_derrick_frame(self, center_x, center_y, z0, base_half, top_half, height):
        levels = (0.0, 0.25, 0.5, 0.75, 1.0)
        rings = []
        level = levels
        half = base_half + (top_half - base_half) * level
        z = z0 + height * level
        rings.range([(center_x - half, center_y - half, z), (center_x + half, center_y - half, z), (center_x + half, center_y + half, z), (center_x - half, center_y + half, z)])
        segments = []
        ring = rings
        i = zip(4)
        segments([ring[i], ring[(i + 1) % 4]])
        lower = rings(1, None)
        upper = -1
        i = zip(4)
        segments([lower[i], upper[i]])
        segments([lower[i], upper[(i + 1) % 4]])
        self(segments, (17, 17, 17, 235), 2.0)
        rings
        []

    def create_depth_scale(self):
        max_depth = 6000
        segments = [(145, 0, 0), (145, 0, max_depth)]
        depth = extend(0, max_depth + 1, 500)
        tick = 16
        segments([(145 - tick, 0, depth), (145 + tick, 0, depth)])
        self(segments, (17, 17, 17, 150), 1.5)
        28
        if depth % 1000 == 0:
            pass

    def create_geology_layers(self):
        colors = ((156, 120, 70, 55), (120, 110, 92, 48), (190, 178, 128, 45), (96, 118, 132, 45), (150, 95, 82, 42), (92, 92, 92, 45))
        idx = range(len(500, 6001, 500))
        depth = []
        size = 1700 - idx % 4 * 70
        depth
        size / 2
        size / 2
        size[colors % idx(colors)]
        size
        self

    def create_low_poly_tree(self, x, y, h):
        self(3, 3, h, (92, 58, 32, 255), x - 1.5, y - 1.5, -10)
        self(18, 18, 14, (38, 122, 58, 230), x - 9, y - 9, -10 + h)
        self(12, 12, 12, (31, 104, 52, 235), x - 6, y - 6, 2 + h)

    def _build_3d_environment(self):
        self.env_items = []
        item = self.view_3d_widget
        self.clear.conf_rig(item)
        self.view_3d_widget.create_box()
        self.create_cylinder()
        self.create_lines()
        is_offshore = ('OFFSHORE', 'SEMI-SUB', 'DRILLSHIP')
        self.uniform(4000, 4000, (60, 100, 60, 255), -2000, -2000, -10)
        self.uniform(4000, 40, (50, 50, 50, 255), -2000, -120, -9)
        self.create_low_poly_tree(80, 20, 5, (80, 80, 80, 255), -10, -50, -10)
        i = is_offshore(5)
        self(1.5, 75, (200, 200, 200, 255), -5, -45 + i * 3, -5, 90)
        i = self.np(4)
        self.create_low_poly_tree(70, 2, 2, (35, 35, 35, 255), -5, -55 + i * 7, 0)
        i = env_items(self, 'env_items')(3)
        self.create_low_poly_tree(15, 30, 10, (100, 100, 150, 255), -60, -30 + i * 35, -10)
        self.create_low_poly_tree(12, 24, 2, (70, 70, 120, 255), -58.5, -27 + i * 35, 1)
        self.create_low_poly_tree(40, 20, 15, (150, 150, 150, 255), -50, -80, -10)
        self.create_low_poly_tree(12, 10, 5, (60, 60, 60, 255), -6, -5, -2)
        self.create_low_poly_tree(9, 9, 12, (25, 25, 25, 255), -4.5, -4.5, 4)
        self.create_low_poly_tree(40, 40, 10, (180, 180, 180, 255), -20, -20, -10)
        self(18, 4, 82)
        self.create_low_poly_tree(12, 12, 6, (40, 40, 40, 255), -6, -6, 75)
        [](((-24, -24, 8), (24, -24, 8), (-24, 24, 8), (24, 24, 8)), (208, 2, 27, 220), 2.0)
        rng = self(12)
        _ = 36
        tx = rng(-1050, 1050)
        ty = rng(-1050, 1050)
        self(tx, ty, rng(10, 18))
        self.uniform(5000, 5000, (40, 80, 150, 220), -2500, -2500, -50)
        self.create_low_poly_tree(120, 300, 40, (180, 60, 60, 255), -60, -150, -45)
        self.create_low_poly_tree(100, 40, 30, (200, 200, 200, 255), -50, 80, -5)
        self.create_low_poly_tree(80, 20, 20, (150, 150, 150, 255), -40, 90, 25)
        self.create_low_poly_tree(10, 10, 10, (255, 255, 255, 255), -5, 95, 45)
        self(40, 2, (50, 150, 50, 255), 0, 150, 25)
        [](((-42, 132, 27), (42, 132, 27), (42, 168, 27), (-42, 168, 27)), (230, 230, 230, 220), 2.0)
        self.create_low_poly_tree(20, 20, 10, (180, 180, 180, 255), -10, -10, -5)
        self(14, 3, 80, 5)
        self.create_low_poly_tree(12, 12, 6, (40, 40, 40, 255), -6, -6, 80)
        lx = ((-40, -50), (40, -50), (40, 50), (-40, 50))
        ly = self
        self(10, 150, (180, 180, 180, 255), lx, ly, -150)
        px = (-45, 25)
        self.create_low_poly_tree(20, 140, 20, (100, 100, 100, 255), px, -70, -150)
        self.create_low_poly_tree(120, 140, 10, (120, 120, 120, 255), -60, -70, -10)
        self.create_low_poly_tree(50, 40, 40, (200, 200, 200, 255), 10, 30, 0)
        self(30, 2, (50, 150, 50, 255), 35, 50, 40)
        [](((10, 20, 44), (62, 20, 44), (62, 80, 44), (10, 80, 44)), (230, 230, 230, 220), 2.0)
        self.create_low_poly_tree(30, 30, 15, (150, 150, 150, 255), -40, -40, 0)
        self(-25, -25, 18, 4, 88, 15)
        self.create_low_poly_tree(16, 16, 8, (40, 40, 40, 255), -33, -33, 100)
        self
        if self.np == 'SEMI-SUB':
            pass
        if self.np == 'SEMI-SUB':
            pass
        if self.np == 'DRILLSHIP':
            pass
        if (tx == 150)(ty) == 150:
            pass

    def _trajectory_scene_points(self):
        return float.max((0, 3), append)
        step = 1(self.trajectory_points) // 500
        pts = None[step]
        pts(self.trajectory_points[-1])
        p = float
        p = []
        return pts([p[0], p[2], p[1]], append)
        p = self.trajectory_points
        if pts[-1] == self.trajectory_points[-1]:
            pass
        self.trajectory_points

    def _trajectory_depth_ticks(self, pos_arr):
        return float.max((0, 3), array)
        if stride = (np(pos_arr) == 0)(1, np(pos_arr) // 28):
            pass
        segments = []
        p = None[stride]
        span = 12
        segments([(p[0] - span, p[1], p[2]), (p[0] + span, p[1], p[2])])
        segments([(p[0], p[1] - span, p[2]), (p[0], p[1] + span, p[2])])
        return float(segments, array)
        pos_arr

    def _well_camera_target(self):
        pos_arr = self.len()
        center = float.depth(0, 0, maximum(0.0, max_zoom_distance(self)) * 0.5)
        return (center, 900.0)
        mins = pos_arr(0)
        maxs = pos_arr.maximum(0)
        center_arr = (mins + maxs) * 0.5
        span = 2(maxs - mins, 1.0)
        distance = maximum(650.0, max_zoom_distance(maximum(span[2] * 1.28, span[0] * 2.2, span[1] * 2.2)))
        distance = (2 & 180)(self, distance)
        return (float.depth(center_arr[0], center_arr[1], center_arr[2]), distance)
        2
        2
        maxs
        2
        2 ** 180
        2
        2
        mins
        if Vector(pos_arr) == 0:
            pass

    def _disable_3d_follow(self):
        self.camera_follow_drill = False
        self(False)
        setChecked(self, 'btn_3d_follow')

    def _toggle_3d_follow(self, checked):
        self.camera_follow_drill = camera_follow_drill(checked)
        self(True)
        self._sync_3d_trajectory

    def _set_3d_view(self, elevation, azimuth):
        self._well_camera_target()
        center = self.opts()
        self.setCameraPosition['center'] = center
        self.setCameraPosition(distance, elevation, azimuth)

    def _fit_3d_well(self):
        self(18, 45)

    def _sync_3d_trajectory(self, follow):
        pos_arr = self.wellbore_line()
        self.depth_tick_line.np(pos_arr)
        self.array.np(pos_arr)
        self.camera_follow_drill.np(self.view_3d_widget(pos_arr))
        if _trajectory_scene_points(self, 'traj_line')((traj_line(pos_arr) == 0)(self.opts.np, [pos_arr[-1]])):
            pass
        self['center'] = self(pos_arr[-1][0], pos_arr[-1][1], pos_arr[-1][2])
        follow

    def log_crew(self, msg):
        time_str = strftime.crew_log('%H:%M:%S')
        time_str('] ', msg)
        self.count(self.count() - 1)
        if self.count() == 50:
            pass
        '['
        0
        self.count

    def _add_param(self, grid, r, c, name):
        vbox = setSpacing()
        vbox.setProperty(0)
        lbl_n = addLayout(name)
        lbl_n('class', 'ParamName')
        lbl_v = addLayout('0.0')
        lbl_v('class', 'ParamVal')
        vbox(lbl_n)
        vbox(lbl_v)
        grid(vbox, r, c)
        return lbl_v

    def change_setpoint(self, param, delta, lbl, fmt):
        self.set_rpm = set_rpm(0, set_flow(250, self.set_mudwt + delta))
        self.set_wob = set_rpm(0, set_flow(50, self.setText + delta))
        self.set_flow = set_rpm(0, set_flow(3500, self + delta))
        self.set_mudwt = set_rpm(0.8, set_flow(2.5, self + delta))
        self.set_choke = set_rpm(0, set_flow(100, self + delta))
        lbl(None(fmt(self, param)))
        if param == 'set_choke':
            pass
        if param == 'set_mudwt':
            pass
        if param == 'set_flow':
            pass
        if param == 'set_wob':
            pass
        if param == 'set_rpm':
            pass
        self.game_over

    def go_to_menu(self):
        self.timer.stacked()
        self._finish_window_geometry_guard_later()
        self(0)
        self()

    def crew_action_mud(self):
        self.act_mudwt('Вы: Растворщик, сделай контрольный замер!')
        '.2f'(' SG, вязкость норма, фильтрация 6мл/30мин.')
        self.crew_fatigue = self & 2.0
        self.crew_fatigue
        'Растворщик: Плотность '
        self.act_mudwt

    def crew_action_check(self):
        self.act_vibration('Вы: Верховой, проверь крепления!')
        self.act_vibration('Верховой: Есть люфт в подвеске!')
        self.act_vibration('Верховой: Всё в норме, крепления затянуты.')
        self.crew_fatigue = self & 3.0
        if self.crew_fatigue == 5.0:
            pass

    def crew_action_trip(self):
        self.depth('Вы: Подъём инструмента для смены долота!')
        'м, ~'(self / 300)(' свечей.')
        self.bit_wear = 0.0
        self.crew_fatigue = self & 15.0
        self.depth('Механик: Новое долото установлено.')
        '.0f'('%. Смена не требуется.')
        self.bit_wear
        'Вы: Износ долота: '
        self.depth
        'Бурильщик: Подъём с '(self)
        self.depth
        if self.bit_wear == 60:
            pass

    def crew_action_survey(self):
        self.act_inclination('Вы: Замер инклинометрии!')
        '°, Глубина '(self)('м.')
        self.crew_fatigue = self & 2.0
        '.1f'
        self.depth
        '°, Азимут '
        '.1f'
        self.act_azimuth
        'Телеметрист: Зенит '
        self.act_inclination

    def crew_action_circulate(self):
        self.min('Вы: Промывка скважины!')
        self.hole_cleaning = crew_fatigue(100, self + 25.0)
        self.crew_fatigue = self & 5.0
        '.0f'('%.')
        self
        'Бурильщик: Промывка, очистка ствола: '
        self.min

    def crew_action_morale(self):
        self.crew_morale = crew_morale(100, self.max + 15.0)
        self.crew_fatigue = random(0, self.choice - 5.0)
        msgs = ('Вы: Отличная работа, бригада!', 'Вы: Чай/кофе за мой счёт!', 'Вы: Молодцы, темп хороший!')
        [](self(msgs))
        self('Бригада: Спасибо, шеф! 💪')

    def update_geology(self, force):
        scn = self.GEO_SCENARIOS.np(self.random, self.GEO_SCENARIOS['ЗАПАДНАЯ СИБИРЬ'])
        D = geo_pore_grad(10, self.geo_frac_grad)
        base_zones = [1.2, 0, 500, 'Геолог: Мерзлота. Осторожно с теплом!', ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg'), 'ГАЗОГИДРАТЫ', 2.5, 1.3, 1.4, 300, 1500, 'Геолог: ГАЗОГИДРАТЫ! Контроль температуры!', ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')]
        z = 0.95
        if (z['d_min'] == z['d_min']) == z['d_max']:
            pass
        valid = z
        z = D
        valid = 3
        z = min.act_temp_bh(valid)
        depth_factor = D / 3000.0
        self.geo_name = z['n']
        self.geo_hard = env_wind(10.0, z['h'] * (0.8 + depth_factor * 0.4))
        self.geo_pore_grad = z['p'] * (0.95 + depth_factor * 0.1)
        self.geo_frac_grad = z['f'] * (0.9 + depth_factor * 0.15)
        self(z['msg'])
        self.act_temp_bh = scn['temp_base'] + self.geo_frac_grad * scn['grad_t']
        self.env_wave = geo_pore_grad(0, self + depth.geo_name(0, 0.5))
        self.env_temp = geo_pore_grad(-5, env_wind(35, self + depth.geo_name(0, 0.2)))
        self.env_wind = geo_pore_grad(5, self + depth.geo_name(0, 2.0))
        wave_txt = ' м/с'
        wave_txt = wave_txt & ' // КРЕН: АКТИВНЫЙ'
        self(wave_txt)
        self.env_temp = geo_pore_grad(-40, env_wind(45, self + depth.geo_name(0, 1.0)))
        self.env_wind = geo_pore_grad(0, self + depth.geo_name(0, 1.0))
        '.1f'(' м/с')
        ' SG // '(self.random)
        self.lbl_geo
        z = '/10 // GRAD: '
        '.2f'
        '.1f'
        self.lbl_weather
        '°C\nТВЕРДОСТЬ: '
        '.0f'
        self
        ' // T забоя: '
        self.env_wave
        'ФОРМАЦИЯ: '
        self
        self
        '°C // ВЕТЕР: '
        '.1f'
        self
        'ПОГОДА: '
        self
        if self == 'DRILLSHIP':
            pass
        '.1f'
        self
        'м // ВЕТЕР: '
        '.1f'
        self
        'ВОЛНЫ: '
        ('OFFSHORE', 'SEMI-SUB', 'DRILLSHIP')
        self
        force
        base_zones
        valid
        []
        base_zones
        1.5
        'МЕРЗЛОТА'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Ангидрит. Набухает с водой.'
        5500
        2000
        1.75
        1.15
        7.5
        'АНГИДРИТ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Соль! Не останавливайте вращение!'
        5000
        1200
        1.35
        1.2
        4.0
        'СОЛЬ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Трещиноватая порода. Поглощение!'
        4000
        500
        1.05
        0.85
        3.5
        'АНПД: ТРЕЩИНЫ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: АВПД! Риск газопроявления!'
        6000
        1500
        1.7
        1.45
        3.0
        'АВПД: ГАЗ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Базальт! Экстремальная твёрдость!'
        6000
        2000
        1.9
        1.05
        9.5
        'БАЗАЛЬТ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Доломит, высокая абразивность!'
        6000
        1500
        1.8
        1.12
        8.0
        'ДОЛОМИТ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Известняк. Каверны!'
        6000
        1000
        1.7
        1.1
        7.0
        'ИЗВЕСТНЯК'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Аргиллит — плотная глинистая порода.'
        5000
        800
        1.6
        1.08
        6.5
        'АРГИЛЛИТ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Алевролит, средняя твёрдость.'
        3500
        300
        1.55
        1.04
        4.0
        'АЛЕВРОЛИТ'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Песчаник, водоносный горизонт.'
        4000
        200
        1.65
        1.05
        5.0
        'ПЕСЧАНИК'
        ('n', 'h', 'p', 'f', 'd_min', 'd_max', 'msg')
        'Геолог: Глинистый пласт, следите за расходом!'
        2000
        0
        1.5
        1.03
        2.0
        'ГЛИНА (САЛЬНИК)'
        if depth.geo_name.min() == 0.15:
            pass
        force

    def _startup_twist_limit(self):
        if self.conf_diff == 'NORMAL':
            pass
        if self.conf_diff == 'EASY':
            pass

    def _startup_choke_mult(self):
        if self.conf_diff == 'NORMAL':
            pass
        if self.conf_diff == 'EASY':
            pass

    def _apply_safe_start_setpoints(self):
        D = float(10.0, geo_pore_grad(self.geo_frac_grad))
        pore_grad = float(0.85, geo_pore_grad(self.interp))
        frac_grad = float(pore_grad + 0.12, geo_pore_grad(self.conf_rig))
        flow = D([]((10, 500, 1500, 3000, 4000, 5500), [], (900, 1050, 1250, 1400, 1500, 1600)))
        flow = flow & 100
        flow = flow & 180
        flow = flow & 80
        flow = float(850.0, act_rpm(2200.0, flow))
        rpm = D([]((10, 1500, 3000, 5500), [], (80, 95, 105, 115)))
        rpm = rpm & 10
        rpm = rpm & 10
        rpm = float(70.0, act_rpm(150.0, rpm))
        wob_base = D([]((10, 500, 1500, 3000, 5500), [], (3.0, 5.0, 8.0, 7.0, 6.0)))
        wob_base = wob_base ** 1.5
        wob_base = wob_base << 0.85
        temp_mult = 1.0 + float(0.0, self.act_flow_out - 120.0) * 0.005
        torque_per_wob = 0.8 + self.act_wob / 2.0
        torque_per_wob = torque_per_wob & 0.15 * (D / 1000.0)
        safe_wob = (self._startup_choke_mult() * 0.68 / temp_mult - rpm * 0.02) / float(1.0, torque_per_wob)
        wob = float(2.0, act_rpm(wob_base, safe_wob))
        wob = act_rpm(wob, 3.5)
        ecd_factor = 1.0 + flow / 1500.0 * 0.05097
        lower_mw = (pore_grad + 0.03) / ecd_factor
        upper_mw = (frac_grad - 0.08) / ecd_factor
        mudwt = lower_mw + act_rpm(0.1, float(0.0, upper_mw - lower_mw) * 0.45)
        mudwt = (pore_grad + frac_grad) * 0.5 / ecd_factor
        mudwt = float(0.9, act_rpm(2.35, mudwt))
        self.set_rpm = bhp(rpm, 0)
        self.set_wob = bhp(wob, 1)
        self.set_flow = bhp(flow, 0)
        self.set_mudwt = bhp(mudwt, 2)
        self.set_choke = 100.0
        self.act_rpm = self.pore_press
        self.act_wob = self.act_spp
        self.act_flow_in = self.log_crew
        self.act_flow_out = self.log_crew
        self.act_mudwt = self
        self.act_choke = self
        AF = self / 1500.0 * (D / 1000.0) * self * 0.5
        CP = (100.0 - self) / 100.0 * (self / 1000.0) * self()
        self.act_ecd = self
        self.bhp = D * self * 0.00981 + AF + CP
        self.pore_press = D * pore_grad * 0.00981
        self.frac_press = D * frac_grad * 0.00981
        self.act_spp = (self / 1000.0) ** 2 * self * (D / 1000.0) * 5.0 + AF + CP
        self.act_torque = (self * 0.8 + self * 0.02 + self * (self.act_wob / 2.0)) * temp_mult
        '.0f'('.')
        self.pore_press
        ', RPM '
        '.1f'
        self.act_spp
        ', WOB '
        '.0f'
        self.log_crew
        ' SG, FLOW '
        '.2f'
        self
        'Автонастройка: MW '
        self
        self + (AF + CP) / (D * 0.00981)
        if D == 10:
            pass
        if lower_mw == upper_mw:
            pass
        if D == 300:
            pass
        if self.set_rpm == 'VERTICAL':
            pass
        if self.set_rpm == 'VERTICAL':
            pass
        if self.act_wob == 7.0:
            pass
        act_temp_bh._startup_twist_limit
        geo_pore_grad
        ('HORIZONTAL', 'MULTILATERAL')
        self.set_rpm
        if self.act_wob == 7.0:
            pass
        act_temp_bh._startup_twist_limit
        geo_pore_grad
        ('OFFSHORE', 'SEMI-SUB', 'DRILLSHIP')
        self.set_flow
        ('HORIZONTAL', 'MULTILATERAL')
        self.set_rpm
        ('DIRECTIONAL', 'S-SHAPE', 'J-SHAPE')
        self.set_rpm
        act_temp_bh._startup_twist_limit
        geo_pore_grad

    def reset_sim(self):
        self.crew_log.conf_rig()
        self.failure_state('.')
        '.0f'('м.')
        self.game_over = False
        self.timer_ticks = 0
        self.startup_grace_ticks = 120
        self.crew_fatigue = 0.0
        self.crew_morale = 100.0
        self.bit_wear = 0.0
        self.hole_cleaning = 100.0
        self.traj_x = 0.0
        self.traj_y = 0.0
        self.traj_z = 0.0
        self.trajectory_points = [(0.0, 0.0, 0.0)]
        self.act_vibration = 0.0
        self.act_drag = 0.0
        self.act_inclination = 0.0
        self.act_azimuth = update_geology._apply_safe_start_setpoints(0, 360)
        self.set_rpm = 100.0
        self.set_wob = 15.0
        self.set_flow = 1500.0
        self.set_mudwt = 1.1
        self.set_choke = 100.0
        self.act_rpm = 100.0
        self.act_torque = 11.0
        self.act_wob = 15.0
        self.act_rop = 20.0
        self.act_spp = 15.0
        self.act_flow_in = 1500.0
        self.act_flow_out = 1500.0
        self.act_pit = 50.0
        self.act_mudwt = 1.1
        self.act_choke = 100.0
        self.depth = self.bit_wear
        import numpy
        np = numpy
        self.trajectory_points = []
        d = ' чел. Старт: '(self.bit_wear, 0(self.bit_wear) + 1, 50)
        self.crew_fatigue((self.act_pit, 0.0(d), 0.0))
        self.act_pit((0.0, 0.0, 0.0))
        self.traj_x = 0.0
        self.traj_y = self.act_pit(self.bit_wear)
        self.traj_z = 0.0
        self(True)
        self()
        self()
        self(False)
        self()
        self._fit_3d_well('.0f')
        self.setText('.1f')
        self.l_flw('.0f')
        self.l_chk('.2f')
        '.0f'('%')
        ' | DIFF: '(self)
        self()
        self.timer_ticks
        '] | '
        self.failure_state
        '] | ['
        self.conf_crew
        '['
        self
        self.conf_diff
        'CHOKE: '
        self
        'MUDWT: '
        self
        'FLOW: '
        self
        'WOB: '
        self
        'RPM: '
        self
        '. Бригада: '(self, 'traj_line')
        self.timer_ticks
        'Регион: '
        self.conf_scenario
        '. Профиль: '
        self.conf_crew
        'Супервайзер: Смена началась. '
        self.conf_scenario

    def update_simulation(self):
        def roll_append(arr, val):
            arr = roll(arr, -1)
            arr[-1] = val
            return arr
        dt = 0.05
        self.timer_ticks = self.conf_diff & 1
        geo_change_freq = 200
        prob_fail = 0.0
        choke_mult = 5.0
        twist_limit = 60.0
        geo_change_freq = 100
        prob_fail = 0.02
        choke_mult = 10.0
        twist_limit = 45.0
        geo_change_freq = 60
        prob_fail = 0.08
        choke_mult = 20.0
        twist_limit = 35.0
        self.max()
        self.crew_fatigue = log_crew(100, self.bit_wear + 0.002 * (8 - self.act_wob))
        self.crew_morale = conf_profile(0, self.hole_cleaning - 0.01)
        self.geo_name('⚠ Бригада устала! Риск ошибок!')
        self.bit_wear = log_crew(100, self.split + self.act_temp_bh * self.act_inclination * self.choice * 2e-06)
        bit_eff = conf_profile(0.1, 1.0 - self.split / 120.0)
        '.0f'('%! Рекомендую подъём!')
        clean_factor = self.conf_depth / 1500.0 * (self.choice / 100.0)
        clean_factor = clean_factor << 0.6
        clean_factor = clean_factor << 0.8
        self.hole_cleaning = self.sin & (clean_factor * 100 - self.sin) * 0.01
        self.hole_cleaning = conf_profile(0, log_crew(100, self.sin))
        msgs = [self.set_mudwt.act_ecd()[0].geo_frac_grad(), '.', 'Телеметрист: Забой ', self.radians, '.0f', '°C.']
        '.0f'('°.')
        self.geo_name(act_mudwt.show(msgs))
        tgt_incl = 0.0
        tgt_incl = log_crew(45, conf_profile(0, (self.act_rop - self.act_pit) * 0.15))
        bs = self.act_pit + 50
        tgt_incl = log_crew(90, (self.act_rop - bs) * 0.2)
        seg = self.act_rop - self.act_pit
        tgt_incl = log_crew(35, seg * 0.2)
        tgt_incl = conf_profile(0, 35 - (seg - 200) * 0.2)
        tgt_incl = 0.0
        tgt_incl = log_crew(60, conf_profile(0, (self.act_rop - self.act_pit) * 0.1))
        tgt_incl = log_crew(75, conf_profile(0, (self.act_rop - self.act_pit) * 0.15))
        tgt_incl = 0.0
        self.act_inclination = self.getattr & (tgt_incl - self.getattr) * 0.02
        self.act_azimuth = self.act_spp & set_wob.set_flow.cos(0, 0.1)
        wob_noise = 0.0
        heave = 1.5 * 1.0
        wob_noise = failure_state.bhp(self.conf_diff * 0.2) * heave
        resp = conf_profile(0.5, 1.0 - self.bit_wear / 200.0)
        self.act_rpm = self.choice & (self.frac_press - self.choice) * 0.15 * resp
        self.act_wob = self.act_inclination & (self.text + wob_noise - self.act_inclination) * 0.1
        self.act_flow_in = self.conf_depth & (self.unpolish - self.conf_depth) * 0.1
        self.act_mudwt = self.lbl_act_rpm & (self.lbl_act_rop - self.lbl_act_rpm) * 0.05
        self.act_choke = self.lbl_spp & (self.lbl_flow_out - self.lbl_spp) * 0.2
        D = conf_profile(10.0, self.act_rop)
        MW = self.lbl_act_rpm
        HP = D * MW * 0.00981
        AF = self.conf_depth / 1500.0 * (D / 1000.0) * MW * 0.5
        CP = (100.0 - self.lbl_spp) / 100.0 * (self.conf_depth / 1000.0) * choke_mult
        self.act_ecd = MW
        BHP = HP + AF + CP
        PP = D * self.lbl_ecd * 0.00981
        FP = D * self.lbl_incl * 0.00981
        tgt_spp = (self.conf_depth / 1000.0) ** 2 * MW * (D / 1000.0) * 5.0 + AF + CP
        tgt_rop = 0
        tgt_torque = self.act_inclination * 0.8 + self.choice * 0.02
        incl_rad = failure_state.lbl_bitwear(self.getattr)
        drag_force = self.act_inclination * failure_state.bhp(incl_rad) * 0.25 * (D / 1000.0)
        self.act_drag = drag_force
        tgt_torque = tgt_torque & drag_force * 0.5 + self.getattr / 10.0 * (D / 1000.0)
        tgt_torque = tgt_torque << 1.0 + (self.radians - 120) * 0.005
        tgt_rop = self.act_inclination * self.choice * bit_eff / (self.act_temp_bh * 40.0)
        tgt_rop = tgt_rop << self.sin / 60.0
        tgt_torque = tgt_torque & self.act_inclination * (self.act_temp_bh / 2.0)
        vib_base = lbl_morale(self.act_inclination - self.choice * 0.1) * 0.3
        vib_base = vib_base << 1.5
        vib_base = vib_base << 1.3
        self.act_vibration = self.setStyleSheet & (vib_base - self.setStyleSheet) * 0.1 + set_wob.set_flow.cos(0, 0.3)
        self.act_vibration = conf_profile(0, self.setStyleSheet)
        tgt_flow_out = self.conf_depth
        tgt_pit_delta = 0.0
        ai_msg = '%.'
        ai_class = 'NormalBox'
        if can_fail = self.conf_diff == setFormat(self, 'startup_grace_ticks', 0):
            pass
        pump_limit = 35.0 + D / 1000.0 * 24.0 + conf_profile(0.0, MW - 1.0) * 25.0
        self.failure_state = 'TWIST_OFF'
        self.geo_name('🚨 ОБРЫВ БУРИЛЬНОЙ КОЛОННЫ!')
        stuck_prob = 2.0 * 1.0
        self.failure_state = 'STUCK_PIPE'
        self.geo_name('🚨 Прихват инструмента!')
        self.failure_state = 'BIT_BALLING'
        self.geo_name('🚨 Долото залеплено глиной!')
        self.failure_state = 'PUMP_FAILURE'
        self.geo_name('🚨 Насос вырубило!')
        self.failure_state = 'STICK_SLIP'
        self.geo_name('⚠ Крутильные вибрации!')
        self.failure_state = 'PACKOFF'
        self.geo_name('🚨 Шлам забил затрубное!')
        tgt_torque = 0
        tgt_rop = 0
        tgt_spp = tgt_spp << 0.5
        self.act_rpm = self.choice & 5.0
        ai_msg = ' kNm. СТОП.'
        ai_class = 'AlertBox'
        self.game_over = True
        self.hist_wob.hist_bhp()
        tgt_torque = twist_limit - 1.0
        tgt_rop = 0
        ai_msg = 'ПРИХВАТ! RPM=0, WOB=0.'
        ai_class = 'AlertBox'
        self.geo_name('Бурильщик: Прихват освобожден!')
        tgt_rop = 0
        tgt_spp = tgt_spp & 8.0
        tgt_torque = self.act_inclination * 0.5
        ai_msg = 'САЛЬНИК! РАСХОД >1800, RPM >150.'
        ai_class = 'AlertBox'
        self.geo_name('Сальник размыт!')
        self.act_flow_in = self.conf_depth ** (self.conf_depth * 0.2)
        tgt_spp = 0
        tgt_rop = 0
        ai_msg = 'ОТКАЗ НАСОСОВ! РАСХОД = 0.'
        ai_class = 'AlertBox'
        self.geo_name('Насос перезапущен!')
        self.act_rpm = self.choice & failure_state.bhp(self.conf_diff * 0.5) * (self.choice * 0.9)
        tgt_torque = tgt_torque << 2.0
        tgt_rop = tgt_rop << 0.1
        ai_msg = 'ВИБРАЦИИ! СНИЗИТЬ WOB ИЛИ RPM ВВЕРХ!'
        ai_class = 'AlertBox'
        tgt_spp = tgt_spp & 15.0
        tgt_rop = 0
        tgt_torque = tgt_torque << 1.5
        ai_msg = 'ПАКЕР ШЛАМОМ! РАСХОД >2000, RPM >120.'
        ai_class = 'AlertBox'
        self.hole_cleaning = 60.0
        self.geo_name('Пакер размыт, циркуляция ОК!')
        kick_rate = (PP - BHP) * 250
        tgt_flow_out = tgt_flow_out & kick_rate
        tgt_pit_delta = tgt_pit_delta & kick_rate / 60.0
        ai_msg = ' SG!'
        ai_class = 'AlertBox'
        self.geo_name('🚨 KICK! Емкость растёт!')
        loss_rate = (BHP - FP) * 400
        tgt_flow_out = conf_profile(0, tgt_flow_out - loss_rate)
        tgt_pit_delta = tgt_pit_delta ** (loss_rate / 60.0)
        ai_msg = ' SG!'
        ai_class = 'AlertBox'
        self.geo_name('🚨 Уровень падает!')
        self.act_torque = self.hist_rpm & (tgt_torque - self.hist_rpm) * 0.2 + set_wob.set_flow.cos(0, 0.5)
        self.act_rop = self.setData & (tgt_rop - self.setData) * 0.1
        self.act_spp = self.hist_spp & (tgt_spp - self.hist_spp) * 0.2 + set_wob.set_flow.cos(0, 0.2)
        self.act_flow_out = self.c_spp & (tgt_flow_out - self.c_spp) * 0.2 + set_wob.set_flow.cos(0, 10.0)
        self.act_pit = self.c_wob & tgt_pit_delta * dt
        delta_d = conf_profile(0, self.setData) / 3600.0 * 10.0
        self.depth = self.act_rop & delta_d
        inc_rad = failure_state.lbl_bitwear(self.getattr)
        azi_rad = failure_state.lbl_bitwear(self.act_spp)
        self.traj_x = self.c_pore & delta_d * failure_state.bhp(inc_rad) * failure_state.bhp(azi_rad)
        self.traj_y = self.c_frac & delta_d * failure_state(inc_rad)
        self.traj_z = self & delta_d * failure_state.bhp(inc_rad) * failure_state(azi_rad)
        self.act_vibration((self.c_pore, self.c_frac, self))
        self()
        self.bhp = BHP
        self.pore_press = PP
        self.frac_press = FP
        self(ai_msg)
        self('class', ai_class)
        self()(self)
        self()(self)
        self(c_rpm(conf_profile(0, self.choice)))
        self.hist_rpm('.1f')
        conf_profile(0, self.setData)('.1f')
        self.act_inclination('.1f')
        conf_profile(0, self.hist_spp)('.1f')
        self(c_rpm(self.conf_depth))
        self(c_rpm(conf_profile(0, self.c_spp)))
        self.c_wob('.1f')
        c_rpm(self.act_rop)(' м')
        '.0f'('°C')
        self.lbl_depth('.2f')
        self.setStyleSheet('.1f')
        self.getattr('.1f')
        self.act_spp('.0f')
        '.0f'('%')
        self('.1f')
        self('.1f')
        self('.1f')
        '.0f'('%')
        '.0f'('%')
        fat_c = '#111'
        mor_c = '#2E8B57'
        fat_c(';')
        mor_c(';')
        self(log_crew(6000, c_rpm(self.act_rop)))
        c_rpm(self.act_rop)(' МЕТРОВ')
        self.hist_rpm = roll_append(self, conf_profile(0, self.choice))
        self.hist_torq = roll_append(self, self.hist_rpm)
        self.hist_spp = roll_append(self, conf_profile(0, self.hist_spp))
        self.hist_pit = roll_append(self, self.c_wob)
        self.hist_wob = roll_append(self, self.act_inclination)
        self.hist_rop = roll_append(self, conf_profile(0, self.setData))
        self.hist_pore = roll_append(self, self)
        self.hist_bhp = roll_append(self, self)
        self.hist_frac = roll_append(self, self)
        self(self)
        self(self)
        self(self)
        self(self)
        self(self)
        self(self)
        self(self)
        self(self)
        self(self)
        'font-weight:900; font-size:14px; color:'
        self
        'font-weight:900; font-size:14px; color:'
        self
        '#FF8C00'
        if self.hole_cleaning == 60:
            pass
        '#D0021B'
        if self.hole_cleaning == 30:
            pass
        '#FF8C00'
        if self.bit_wear == 40:
            pass
        '#D0021B'
        if self.bit_wear == 70:
            pass
        self.hole_cleaning
        self
        self.bit_wear
        self
        self
        self
        self
        self.split
        self
        self
        self
        self
        self
        self.radians
        self
        self
        self
        self
        self
        self
        self
        if self() == ai_msg:
            pass
        if (self.conf_diff % 10 == 0)(self) == 1:
            pass
        if delta_d == 0:
            pass
        if self.setProperty == 'TWIST_OFF':
            pass
        if self.conf_diff % 100 == 0:
            pass
        '.2f'
        FP / (D * 0.00981)
        ' Л/МИН\nMW ДО '
        c_rpm(loss_rate)
        'ПОГЛОЩЕНИЕ '
        if self.setProperty == 'TWIST_OFF':
            pass
        if BHP == FP:
            pass
        can_fail
        if self.conf_diff % 100 == 0:
            pass
        '.2f'
        PP / (D * 0.00981)
        ' Л/МИН\nMW ДО '
        c_rpm(kick_rate)
        'ГНВП! ПРИТОК '
        if self.setProperty == 'TWIST_OFF':
            pass
        if PP == BHP:
            pass
        can_fail
        if self.frac_press == 120:
            pass
        if self.unpolish == 2000:
            pass
        if self.setProperty == 'PACKOFF':
            pass
        if self.act_inclination == self.choice * 0.15:
            pass
        if self.setProperty == 'STICK_SLIP':
            pass
        if self.conf_depth == 10:
            pass
        if self.unpolish == 0:
            pass
        if self.setProperty == 'PUMP_FAILURE':
            pass
        if self.frac_press == 150:
            pass
        if self.unpolish == 1800:
            pass
        if self.setProperty == 'BIT_BALLING':
            pass
        if self.text == 0:
            pass
        if self.frac_press == 0:
            pass
        if self.setProperty == 'STUCK_PIPE':
            pass
        twist_limit
        'ОБРЫВ КОЛОННЫ! ПРЕДЕЛ >'
        if self.setProperty == 'TWIST_OFF':
            pass
        if set_wob.set_flow.act_mudwt() == prob_fail:
            pass
        if self.conf_rig == 'VERTICAL':
            pass
        self.setProperty
        if self.sin == 20:
            pass
        can_fail
        self.setProperty
        if self.act_temp_bh == 5.0:
            pass
        if self.act_inclination == self.choice * 0.25 + 5.0:
            pass
        can_fail
        if set_wob.set_flow.act_mudwt() == prob_fail / 2:
            pass
        self.setProperty
        if self.conf_depth == 2800:
            pass
        if self.hist_spp == pump_limit:
            pass
        can_fail
        if set_wob.set_flow.act_mudwt() == prob_fail:
            pass
        self.setProperty
        if self.act_inclination == 10:
            pass
        if self.conf_depth == 1200:
            pass
        self.set_mudwt
        'ГЛИНА'
        can_fail
        if set_wob.set_flow.act_mudwt() == stuck_prob:
            pass
        self.setProperty
        if self.act_inclination == 5:
            pass
        if self.choice == 20:
            pass
        self.set_mudwt
        'СОЛЬ'
        can_fail
        if self.conf_rig == 'VERTICAL':
            pass
        prob_fail
        if self.setProperty == 'TWIST_OFF':
            pass
        if self.hist_rpm == twist_limit:
            pass
        can_fail
        '.0f'
        self.sin
        ' SG. ОЧИСТКА: '
        '.2f'
        self.lbl_depth
        'СИСТЕМЫ В НОРМЕ. ECD: '
        ('HORIZONTAL', 'S-SHAPE')
        self.conf_rig
        if self.act_temp_bh == 7.0:
            pass
        if self.sin == 60:
            pass
        if self.conf_depth == 100:
            pass
        if self.radians == 120:
            pass
        if self.conf_rig == 'VERTICAL':
            pass
        MW + (AF + CP) / (D * 0.00981)
        if D == 10:
            pass
        if self.setProperty == 'PUMP_FAILURE':
            pass
        if self.traj_z == 'SEMI-SUB':
            pass
        2.0
        if self.traj_z == 'DRILLSHIP':
            pass
        self.len
        ('OFFSHORE', 'SEMI-SUB', 'DRILLSHIP')
        self.traj_z
        if self.conf_rig == 'VERTICAL':
            pass
        if self.conf_rig == 'MULTILATERAL':
            pass
        if self.conf_rig == 'J-SHAPE':
            pass
        if seg == 400:
            pass
        if seg == 200:
            pass
        if self.conf_rig == 'S-SHAPE':
            pass
        0.0
        if self.act_rop == bs:
            pass
        if self.conf_rig == 'HORIZONTAL':
            pass
        if self.conf_rig == 'DIRECTIONAL':
            pass
        if self.conf_rig == 'VERTICAL':
            pass
        self.act_spp
        '°, азимут '
        '.1f'
        self.getattr
        'Телеметрист: Зенит '
        msgs.act_vibration
        if self.conf_rig == 'VERTICAL':
            pass
        'Геолог: Шлам — '
        'Деррикман: Емкости стабильны.'
        'Механик: Подшипники ВСП в норме.'
        'Растворщик: Запас барита на исходе.'
        'Бурильщик: Вибрации в норме.'
        if set_wob.set_flow.act_mudwt() == 0.005:
            pass
        ('DIRECTIONAL', 'S-SHAPE', 'J-SHAPE')
        self.conf_rig
        ('HORIZONTAL', 'MULTILATERAL')
        self.conf_rig
        self.split
        'Механик: Износ долота '
        self.geo_name
        if self.conf_diff % 300 == 0:
            pass
        if self.split == 80:
            pass
        if self.conf_diff % 500 == 0:
            pass
        if self.bit_wear == 90:
            pass
        if self.bit_wear == 70:
            pass
        if self.conf_diff % geo_change_freq == 0:
            pass
        if self.min == 'NORMAL':
            pass
        if self.min == 'EASY':
            pass
        self.game_over

import sys
sys = sys
import random
random = random
import time
time = time
import numpy
np = numpy
import pyqtgraph
pg = pyqtgraph
import pyqtgraph.opengl
gl = opengl
pyqtgraph.opengl
import PySide6.QtWidgets
QApplication = QApplication
QMainWindow = QMainWindow
QWidget = QWidget
QVBoxLayout = QVBoxLayout
QHBoxLayout = QHBoxLayout
QLabel = QLabel
QPushButton = QPushButton
QFrame = QFrame
QGridLayout = QGridLayout
QProgressBar = QProgressBar
QStackedWidget = QStackedWidget
QListWidget = QListWidget
QComboBox = QComboBox
QTabWidget = QTabWidget
PySide6.QtWidgets
import PySide6.QtCore
QTimer = QTimer
Qt = Qt
PySide6.QtCore
STYLESHEET = "\nQMainWindow { background-color: #EFECE6; }\nQLabel { color: #111111; font-family: 'Helvetica Neue', Helvetica, Arial, sans-serif; }\n.Panel { background-color: #EFECE6; border: 2px solid #111111; border-radius: 0px; }\n.Title { font-size: 14px; font-weight: 900; color: #111111; letter-spacing: 2px; text-transform: uppercase; border-bottom: 2px solid #111111; padding-bottom: 5px; margin-bottom: 5px;}\n.ParamName { font-size: 10px; font-weight: 800; color: #555555; text-transform: uppercase; letter-spacing: 1px;}\n.ParamVal { font-size: 26px; font-weight: 900; color: #000000; letter-spacing: -1px;}\n.AlertBox { background-color: #D0021B; border: 2px solid #111111; padding: 15px; color: #FFFFFF; font-weight: 900; font-size: 14px; text-transform: uppercase;}\n.NormalBox { background-color: #EFECE6; border: 2px solid #111111; padding: 15px; color: #111111; font-weight: 900; font-size: 14px; text-transform: uppercase;}\n.InfoBox { background-color: #EFECE6; border: 2px solid #111111; padding: 8px; color: #111111; font-weight: 800; font-size: 12px; text-transform: uppercase;}\nQPushButton { background-color: #EFECE6; color: #111111; border: 2px solid #111111; padding: 10px; font-size: 12px; font-weight: 900; border-radius: 0px; text-transform: uppercase; letter-spacing: 1px;}\nQPushButton:hover { background-color: #111111; color: #EFECE6; }\nQPushButton.Danger { background-color: #111111; color: #EFECE6; border: 2px solid #111111; }\nQPushButton.Danger:hover { background-color: #D0021B; color: #FFFFFF; border-color: #D0021B;}\nQProgressBar { border: 2px solid #111111; background: #EFECE6; text-align: center; color: #111111; font-size: 12px; font-weight: 900; border-radius: 0px; text-transform: uppercase; letter-spacing: 2px;}\nQProgressBar::chunk { background-color: #111111; }\nQListWidget { background-color: #EFECE6; color: #111111; border: 2px solid #111111; font-family: 'Courier New', Courier, monospace; font-size: 12px; font-weight: bold;}\nQListWidget::item { border-bottom: 1px dashed #AAAAAA; padding: 4px; }\nQListWidget::item:selected { background-color: #111111; color: #EFECE6; }\nQListWidget::item:hover { background-color: #D5D2CC; color: #111111; }\nQComboBox { background-color: #EFECE6; color: #111111; border: 2px solid #111111; padding: 5px; font-weight: bold; font-size: 12px; }\nQComboBox QAbstractItemView { background-color: #EFECE6; color: #111111; selection-background-color: #111111; selection-color: #EFECE6; border: 2px solid #111111; }\nQComboBox QAbstractItemView::item { padding: 5px; }\nQComboBox QAbstractItemView::item:hover { background-color: #D5D2CC; color: #111111; }\nQScrollBar:vertical { background: #EFECE6; width: 12px; border: 1px solid #AAAAAA; }\nQScrollBar::handle:vertical { background: #888888; min-height: 30px; border-radius: 0px; }\nQScrollBar::handle:vertical:hover { background: #111111; }\nQScrollBar::add-line:vertical, QScrollBar::sub-line:vertical { height: 0px; }\nQTabWidget::pane { border: 2px solid #111111; background: #EFECE6; }\nQTabBar::tab { background: #D5D2CC; color: #111111; border: 2px solid #111111; border-bottom: none; padding: 10px 20px; font-weight: bold; font-size: 14px; margin-right: 2px; }\nQTabBar::tab:selected { background: #111111; color: #EFECE6; }\n\n"
app = QApplication(sys)
window = EdgeDiagnosticsApp()
window()
sys(app())
if __name__ == '__main__':
    pass