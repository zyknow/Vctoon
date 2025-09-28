/**
 * 通用：根据是否编辑与实体名称构建“编辑/创建”标题。
 * 调用方需传入已翻译的文案（避免该工具直接依赖 i18n 环境）。
 * 约定：编辑存在名称 => `${editLabel}-${name}`；否则回退 editLabel；创建回退 createLabel。
 */
export function buildEditCreateTitle(
  isEdit: boolean,
  entityName: null | string | undefined,
  editLabel: string,
  createLabel: string,
) {
  if (isEdit && entityName) return `${editLabel}-${entityName}`
  return isEdit ? editLabel : createLabel
}
