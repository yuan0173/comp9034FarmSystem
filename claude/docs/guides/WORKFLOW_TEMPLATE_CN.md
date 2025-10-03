# 标准模板工作流（项目内通用）

## 适用范围
- 适用于：后端修复（数据库/迁移/CORS/种子）、前端修复（日期格式、过滤逻辑）、文档新增
- 目标：按规范新建分支 → 实施改动 → 规范提交/PR → 合并 → 验证 → 清理

## 前置条件
- GitHub CLI 已登录：
  - `gh auth login --hostname github.com --scopes repo --git-protocol https --web`
- 本地仓库同步：
  - `git fetch origin --prune`
  - `git checkout main && git pull`
- 无本地锁：
  - `rm -f .git/index.lock`

## 流程强制第一步（中文评审与授权）
- 每次改动前，必须先用中文向仓库所有者提交“变更分析与方案”（问题、根因、候选方案、推荐方案、影响范围、验证计划）。
- 等待仓库所有者“中文确认 + 授权”后，方可进入后续的分支、提交、PR 等操作。
- PR 描述中需包含“已完成中文方案确认并获授权”的说明（或勾选 PR 模板中的对应项）。

## 沟通与解释风格（强制：通俗大白话）
- Chat 沟通一律使用“通俗大白话”解释方式，便于 BA/PO/非技术同学快速理解：
  - 标准输出结构：
    - “已实现了什么（通俗版）”：用 3–6 条短句概括本次改动给用户/业务带来的直接效果（例如：先打入卡再打出卡、同一分钟不让重复打卡、没有排班不让打卡但管理员可放行等）。
    - “下一步我准备做什么（通俗版）”：用 3–6 条短句说明下个迭代将实现的界面/交互/规则（例如：新增排班页面、Station 错误提示与管理员覆盖入口等）。
  - 要求：
    - 避免堆砌技术名词；每条一句话，直指业务效果或用户感知。
    - 如为 BA 文档项（FR/AC）落地，请在 PR 正文单独列出“对照清单”，逐条标注 F?-FR?/AC?，便于验收逐项对勾。
    - 所有解释优先站在“用户/业务”的角度描述，不纠缠内部实现细节。
  - 样例（片段）：
    - 已实现了什么（通俗版）：
      - 没排班就不让打卡（除非管理员放行并填原因）。
      - 同一分钟内不让重复打卡，避免误触多记。
    - 下一步我准备做什么（通俗版）：
      - 管理端新增“排班页面”，支持新增/编辑/删除与防重叠。
      - Station 显示后端错误码的友好提示，管理员可在页面直接覆盖打卡。

## 分支命名（规范）
- 新功能：`feat/yuan0173/<scope>/<kebab-描述>`
- 缺陷修复：`fix/yuan0173/<scope>/<kebab-描述>`
- 文档：`docs/yuan0173/docs/<kebab-描述>`
- 示例：
  - `feat/yuan0173/frontend/english-date-format-ddmmyy`
  - `fix/yuan0173/frontend/admin-staffs-active-tab-filter`
  - `docs/yuan0173/docs/production-test-summary-2025-09-30`

## 实施修改（示例要点）
- 前端日期格式（DD/MM/YY、24 小时）：
  - 文件：`frontendWebsite/src/utils/time.ts`
  - `formatDate` → en-GB，`DD/MM/YY`（2 位年份）
  - `formatTime` → `HH:MM`（24h，不含秒）
  - `formatDateTime` → 组合输出 `DD/MM/YY HH:MM`
- AdminStaffs 过滤与 UI：
  - 文件：`frontendWebsite/src/pages/AdminStaffs.tsx`
  - Active 标签页：仅显示 `isActive===true`
  - Inactive 标签页：使用独立 API 数据集（`/api/Staffs/inactive`）
  - 锁定 Status 下拉：随标签显示 Active/Inactive，并禁用（避免误导）
- 后端（示例）：
  - 迁移/索引：PostgreSQL 过滤索引需引用列名（`"Username"`）
  - 种子：幂等补齐 `9001/8001/1001/2001`
  - 控制器：`StaffsController` 注入 `ApplicationDbContext` 并恢复编译
- 文档：
  - 路径：`claude/docs/...`（优先正常纳管，不使用 `-f`；仅当确因 `.gitignore` 规则排除时再使用 `git add -f`，并在 PR 中说明原因）

## 提交信息（Conventional Commits）
- `feat(frontend): switch date/time locale to en-GB (DD/MM/YY, 24h HH:MM)`
- `fix(frontend): ensure Active Staff tab shows only active staff`
- `feat(frontend): lock Status filter to current tab (Active/Inactive)`
- `fix(database): quote filtered index on Staff.Username to avoid 42703`
- `fix(seed): make staff seeding idempotent and ensure missing test accounts are created`
- `fix(backend): include StaffsController in build and inject ApplicationDbContext`
- `docs: add production environment test summary (YYYY-MM-DD)`
- `chore(frontend): update development environment API base URL to Render backend`

## PR 模板（英文，允许使用 Markdown 结构）
- 首行须注明：`[x] 中文方案已与仓库所有者确认并获得授权`
- Title：`<type(scope): concise description>`
- Summary：说明修复/功能点与原因
- Changes Made：列出文件与关键改动（每文件 1–3 点）
- Affected Components：用户可见影响范围（模块/页面）
- Testing：`[x]` 具体验证用例与预期结果
- Impact：行为/迁移/用户可见影响
- Modules Affected：`backend/` 或 `frontendWebsite/` 或 `docs/`
- Rollback Plan：回退方法（还原提交/禁用开关）

## 标准 Git 流程（每个 PR）
- 新建分支：`git checkout -b <branch-name>`
- 暂存/提交：`git add <files>` → `git commit -m "<title>" -m "<details>"`
- 推送：`git push -u origin <branch-name>`
- 创建 PR：`gh pr create --title "<title>" --body "<body>" --base main --head <branch-name>`
- 合并：`gh pr merge <PR#> --merge --delete-branch --admin`（仅仓库维护者使用；普通贡献者请在 GitHub UI 发起并由维护者合并）

## 合并后验证（后端）
- 生产环境：Render（后端示例：https://comp9034farmsystem-backend.onrender.com）
- Render 部署日志：
  - 解析 PostgreSQL 连接（不打印密钥）
  - 迁移成功（无 42703/“relation 不存在”）
  - 种子：`Seeded X missing staff accounts` 或 `All seed staff accounts already exist`
  - `/health` 返回 200
- CORS：
  - 生产用双下划线数组：`AllowedOrigins__0=https://yuan0173.github.io`
  - curl 预检：204 + 正确 CORS 头
- JWT：`JWT_SECRET_KEY / JWT_ISSUER / JWT_AUDIENCE`（或兼容的 `Jwt__*`）

- Swagger（本地开发环境检查）：
  - Swagger UI 可访问，存在 Bearer 安全定义（Authorize 按钮）
  - 受保护接口在 Swagger 中要求授权后试调

- RBAC 冒烟用例（使用不同角色 Token 调用关键接口）：
  - 未携带 Token 访问受保护接口 → 401 Unauthorized
  - Staff 访问 Admin-only 接口（如 Staff 管理写操作）→ 403 Forbidden
  - Staff 在“打卡站”相关接口（若提供）→ 200 并返回预期结果
  - Admin/Manager 访问事件查询/管理接口 → 200，数据符合权限预期

## 合并后验证（前端）
- 时间格式：`DD/MM/YY HH:MM（24h）` → AdminLoginHistory、Station、NetworkBadge、AdminStaffs、AdminEvents
- AdminStaffs：
  - Active 仅显示在职；Inactive 使用独立数据集
  - Status 下拉随标签显示并禁用
- 刷新缓存或重新部署以生效

## 冲突处理（示例）
- 过滤索引：保留带引号版本 `filter: "\"Username\" IS NOT NULL"`；删除未引号版与冲突标记
- JSON 属性冲突：主属性加 `JsonPropertyName("...")`，Legacy 别名加 `JsonIgnore`

## 分支清理
- 合并后删除远端：`gh pr merge <PR#> --merge --delete-branch --admin`
- 删除本地：`git branch -d <branch-name>`
- 如遇锁：`rm -f .git/index.lock`

## 运维注意
- 迁移：仓库包含 `InitialCreate`；启动先 `Migrate` 再 `Seeding`
- 种子：幂等创建基础账号与设备
- CORS：生产用 `AllowedOrigins__N`（双下划线）；允许凭据时不可使用通配符 `*`
- 日志：不输出敏感信息（仅 host/port/db）

## 回滚策略
- 直接 Revert 对应 PR 或提交
- 牵涉数据库时，确保迁移回退安全或使用稳定快照重部署
- 前端行为回滚：还原提交即可

## 端到端示例
- 前端日期格式：
  - 新建分支 → 修改 `time.ts` → 提交/推送 → 开 PR → 合并 → 验证 UI 时间
- AdminStaffs 活跃过滤 + UI 锁定：
  - 新建分支 → 修改 `AdminStaffs.tsx` → 提交/推送 → 开 PR → 合并 → 验证删除/切换标签

> 说明：本工作流模板为项目通用操作规范。后续开发与修复请严格遵循此流程执行。
