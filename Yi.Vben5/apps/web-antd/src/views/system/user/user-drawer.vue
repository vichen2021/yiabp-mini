<script setup lang="ts">
import type { Role } from '#/api/system/user/model';

import { computed, h, onMounted, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { addFullName, cloneDeep, getPopupContainer } from '@vben/utils';

import { Tag } from 'ant-design-vue';

import { useVbenForm } from '#/adapter/form';
import { configInfoByKey } from '#/api/system/config';
import { postOptionSelect } from '#/api/system/post';
import {
  findUserInfo,
  getDeptTree,
  userAdd,
  userUpdate,
} from '#/api/system/user';
import { defaultFormValueGetter, useBeforeCloseDiff } from '#/utils/popup';
import { authScopeOptions } from '#/views/system/role/data';

import { drawerSchema } from './data';

const emit = defineEmits<{ reload: [] }>();

const isUpdate = ref(false);
const title = computed(() => {
  return isUpdate.value ? $t('pages.common.edit') : $t('pages.common.add');
});

const [BasicForm, formApi] = useVbenForm({
  commonConfig: {
    formItemClass: 'col-span-2',
    componentProps: {
      class: 'w-full',
    },
    labelWidth: 80,
  },
  schema: drawerSchema(),
  showDefaultActions: false,
  wrapperClass: 'grid-cols-2',
});

/**
 * 生成角色的自定义label
 * 也可以用option插槽来做
 * renderComponentContent: () => ({
    option: ({value, label, [disabled, key, title]}) => '',
  }),
 */
function genRoleOptionlabel(role: Role) {
  const found = authScopeOptions.find((item) => item.value === role.dataScope);
  if (!found) {
    return role.roleName;
  }
  return h('div', { class: 'flex items-center gap-[6px]' }, [
    h('span', null, role.roleName),
    h(Tag, { color: found.color }, () => found.label),
  ]);
}

/**
 * 岗位的加载
 */
async function setupPostOptions() {
  try {
    const postListResp = await postOptionSelect();
    console.log(postListResp);
    // 确保返回的是数组
    const postList = Array.isArray(postListResp) ? postListResp : [];
    const options = postList.map((item) => ({
      label: item.postName,
      value: item.id,
    }));
    const placeholder = options.length > 0 ? '请选择' : '暂无可选岗位';
    formApi.updateSchema([
      {
        componentProps: { options, placeholder },
        fieldName: 'postIds',
      },
    ]);
  } catch (error) {
    console.error('加载岗位信息失败:', error);
    // 加载失败时设置空选项
    formApi.updateSchema([
      {
        componentProps: { options: [], placeholder: '加载岗位失败' },
        fieldName: 'postIds',
      },
    ]);
  }
}

/**
 * 初始化部门选择
 */
async function setupDeptSelect() {
  try {
    // updateSchema
    const deptTree = await getDeptTree();
    // 确保返回的是数组
    const deptList = Array.isArray(deptTree) ? deptTree : [];
    // 选中后显示在输入框的值 即父节点 / 子节点
    addFullName(deptList, 'label', ' / ');
    formApi.updateSchema([
      {
        componentProps: (formModel) => ({
          class: 'w-full',
          fieldNames: {
            key: 'id',
            value: 'id',
            children: 'children',
          },
          getPopupContainer,
          placeholder: '请选择',
          showSearch: true,
          treeData: deptList,
          treeDefaultExpandAll: true,
          treeLine: { showLeafIcon: false },
          // 筛选的字段
          treeNodeFilterProp: 'label',
          // 选中后显示在输入框的值
          treeNodeLabelProp: 'fullName',
        }),
        fieldName: 'deptId',
      },
    ]);
  } catch (error) {
    console.error('加载部门树失败:', error);
    // 加载失败时设置空树
    formApi.updateSchema([
      {
        componentProps: {
          placeholder: '加载部门失败',
          treeData: [],
        },
        fieldName: 'deptId',
      },
    ]);
  }
}

const defaultPassword = ref('');
onMounted(async () => {
  const password = await configInfoByKey('sys.user.initPassword');
  if (password) {
    defaultPassword.value = password;
  }
});

/**
 * 新增时候 从参数设置获取默认密码
 */
async function loadDefaultPassword(update: boolean) {
  if (!update && defaultPassword.value) {
    formApi.setFieldValue('password', defaultPassword.value);
  }
}

const { onBeforeClose, markInitialized, resetInitialized } = useBeforeCloseDiff(
  {
    initializedGetter: defaultFormValueGetter(formApi),
    currentGetter: defaultFormValueGetter(formApi),
  },
);

const [BasicDrawer, drawerApi] = useVbenDrawer({
  onBeforeClose,
  onClosed: handleClosed,
  onConfirm: handleConfirm,
  async onOpenChange(isOpen) {
    if (!isOpen) {
      // 需要重置岗位选择
      formApi.updateSchema([
        {
          componentProps: { options: [], placeholder: '请选择岗位' },
          fieldName: 'postIds',
        },
      ]);
      return null;
    }
    drawerApi.drawerLoading(true);

    try {
      const { id } = drawerApi.getData() as { id?: number | string };
      isUpdate.value = !!id;
      /** update时 禁用用户名修改 不显示密码框 */
      formApi.updateSchema([
        { componentProps: { disabled: isUpdate.value }, fieldName: 'userName' },
        {
          dependencies: { show: () => !isUpdate.value, triggerFields: ['id'] },
          fieldName: 'password',
        },
      ]);
      // 更新 && 赋值
      const user = await findUserInfo(id);

      // 从用户对象中提取 posts 和 roles
      const posts = user?.posts ?? [];
      const roles = user?.roles ?? [];
      const postIds = posts.map((item: any) => item.id);
      const roleIds = roles.map((item: any) => item.roleId);

      const postOptions = posts.map((item: any) => ({
        label: item.postName,
        value: item.id,
      }));

      formApi.updateSchema([
        {
          componentProps: {
            // title用于选中后回填到输入框 默认为label
            optionLabelProp: 'title',
            options: roles.map((item: any) => ({
              label: genRoleOptionlabel(item),
              // title用于选中后回填到输入框 默认为label
              title: item.roleName,
              value: item.roleId,
            })),
          },
          fieldName: 'roleIds',
        },
        {
          componentProps: {
            options: postOptions,
          },
          fieldName: 'postIds',
        },
      ]);

      // 部门选择、初始密码及用户相关操作并行处理
      const promises = [
        setupDeptSelect(),
        loadDefaultPassword(isUpdate.value),
        setupPostOptions(),
      ];
      if (user) {
        promises.push(
          // 添加基础信息
          formApi.setValues(user),
          // 添加角色和岗位
          formApi.setFieldValue('postIds', postIds),
          formApi.setFieldValue('roleIds', roleIds),
        );
      }
      // 并行处理 重构后会带来10-50ms的优化
      await Promise.all(promises);
      await markInitialized();
    } catch (error) {
      console.error('加载用户信息失败:', error);
    } finally {
      drawerApi.drawerLoading(false);
    }
  },
});

async function handleConfirm() {
  try {
    drawerApi.lock(true);
    const { valid } = await formApi.validate();
    if (!valid) {
      return;
    }
    const data = cloneDeep(await formApi.getValues());
    await (isUpdate.value ? userUpdate(data) : userAdd(data));
    resetInitialized();
    emit('reload');
    drawerApi.close();
  } catch (error) {
    console.error(error);
  } finally {
    drawerApi.lock(false);
  }
}

async function handleClosed() {
  formApi.resetForm();
  resetInitialized();
}
</script>

<template>
  <BasicDrawer :title="title" class="w-[600px]">
    <BasicForm />
  </BasicDrawer>
</template>
