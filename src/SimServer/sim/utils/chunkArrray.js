export function chunkArray(array, minSize = 4, maxSize = 6) {
    const result = [];
    let i = 0;
    while (i < array.length) {
      const remaining = array.length - i;
      const size = Math.min(
        maxSize,
        remaining < minSize ? remaining : Math.floor(Math.random() * (maxSize - minSize + 1)) + minSize
      );
      result.push(array.slice(i, i + size));
      i += size;
    }
    return result;
  }