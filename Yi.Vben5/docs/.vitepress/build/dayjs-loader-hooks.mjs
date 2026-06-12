export async function resolve(specifier, context, nextResolve) {
  if (
    specifier.startsWith('dayjs/plugin/') &&
    !specifier.endsWith('.js')
  ) {
    return nextResolve(`${specifier}.js`, context);
  }

  return nextResolve(specifier, context);
}
