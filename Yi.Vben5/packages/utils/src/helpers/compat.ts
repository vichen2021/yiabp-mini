type TreeOptions = {
  children?: string;
  id?: string;
  pid?: string;
};

type Emitter<Events extends Record<string, any>> = {
  emit<Key extends keyof Events>(type: Key, event?: Events[Key]): void;
  off<Key extends keyof Events>(type: Key, handler?: (event: Events[Key]) => void): void;
  on<Key extends keyof Events>(type: Key, handler: (event: Events[Key]) => void): void;
};

function buildUUID() {
  return crypto.randomUUID();
}

function eachTree<T extends Record<string, any>>(
  tree: T[],
  callback: (item: T, parent?: T) => void,
  options: TreeOptions = {},
) {
  const childrenKey = options.children ?? 'children';
  const walk = (items: T[], parent?: T) => {
    items.forEach((item) => {
      callback(item, parent);
      const children = item[childrenKey];
      if (Array.isArray(children)) {
        walk(children, item);
      }
    });
  };
  walk(tree);
}

function treeToList<T extends Record<string, any>>(
  tree: T[],
  options: TreeOptions = {},
) {
  const childrenKey = options.children ?? 'children';
  const list: T[] = [];
  eachTree(
    tree,
    (item) => {
      list.push(item);
    },
    { children: childrenKey },
  );
  return list;
}

function listToTree<T extends Record<string, any>>(
  list: T[],
  options: TreeOptions = {},
  extra?: Partial<T>,
) {
  const idKey = options.id ?? 'id';
  const pidKey = options.pid ?? 'parentId';
  const childrenKey = options.children ?? 'children';
  const map = new Map<any, T>();
  const roots: T[] = [];

  list.forEach((item) => {
    map.set(item[idKey], item);
    if (!Array.isArray(item[childrenKey])) {
      (item as Record<string, any>)[childrenKey] = [];
    }
    Object.assign(item, extra);
  });

  list.forEach((item) => {
    const parent = map.get(item[pidKey]);
    if (parent && parent !== item) {
      parent[childrenKey].push(item);
    } else {
      roots.push(item);
    }
  });

  return roots;
}

function addFullName<T extends Record<string, any>>(
  tree: T[],
  options: (TreeOptions & { label?: string; separator?: string }) | string = {},
  separatorOrField = 'fullName',
) {
  const normalizedOptions =
    typeof options === 'string' ? { label: options } : options;
  const childrenKey = normalizedOptions.children ?? 'children';
  const labelKey = normalizedOptions.label ?? 'label';
  const separator =
    typeof options === 'string'
      ? separatorOrField
      : (normalizedOptions.separator ?? ' / ');
  const field = 'fullName';

  const walk = (items: T[], parentName = '') => {
    items.forEach((item) => {
      const name = item[labelKey] ?? item.name ?? item.title ?? '';
      (item as Record<string, any>)[field] = parentName
        ? `${parentName}${separator}${name}`
        : name;
      const children = item[childrenKey];
      if (Array.isArray(children)) {
        walk(children, (item as Record<string, any>)[field]);
      }
    });
  };
  walk(tree);
  return tree;
}

function findGroupParentIds<T extends Record<string, any>>(
  tree: T[],
  values: any[],
  options: TreeOptions = {},
) {
  const idKey = options.id ?? 'id';
  const childrenKey = options.children ?? 'children';
  const valueSet = new Set(values);
  const result = new Set<any>();

  const walk = (items: T[], parents: any[]) => {
    items.forEach((item) => {
      const currentParents = [...parents, item[idKey]];
      if (valueSet.has(item[idKey])) {
        parents.forEach((id) => result.add(id));
      }
      const children = item[childrenKey];
      if (Array.isArray(children)) {
        walk(children, currentParents);
      }
    });
  };
  walk(tree, []);
  return [...result];
}

function optionsToEnum<T extends Record<string, any>>(
  options: readonly T[],
  labelKey = 'label',
  valueKey = 'value',
) {
  return options.reduce<Record<string, any>>((acc, item) => {
    acc[item[labelKey]] = item[valueKey];
    return acc;
  }, {});
}

function mitt<Events extends Record<string, any>>(): Emitter<Events> {
  const handlers = new Map<keyof Events, Set<(event: any) => void>>();
  return {
    emit(type, event) {
      handlers.get(type)?.forEach((handler) => handler(event));
    },
    off(type, handler) {
      if (!handler) {
        handlers.delete(type);
        return;
      }
      handlers.get(type)?.delete(handler as (event: any) => void);
    },
    on(type, handler) {
      const currentHandlers = handlers.get(type) ?? new Set();
      currentHandlers.add(handler as (event: any) => void);
      handlers.set(type, currentHandlers);
    },
  };
}

export {
  addFullName,
  buildUUID,
  eachTree,
  findGroupParentIds,
  listToTree,
  mitt,
  optionsToEnum,
  treeToList,
};

export type { Emitter };
