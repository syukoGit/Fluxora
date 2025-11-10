export interface JsonPatchOperation {
  op: 'add' | 'remove' | 'replace' | 'move' | 'copy' | 'test';
  path: string;
  value?: unknown;
  from?: string;
}

export type JsonPatchDocument = JsonPatchOperation[];

/**
 * Creates a JSON Patch document by comparing original and updated objects
 * @param original - The original object state
 * @param updated - The updated object state with changes
 * @returns JSON Patch document following RFC 6902 standard
 */
export function createJsonPatchDocument<T extends Record<string, unknown>>(original: T, updated: T): JsonPatchDocument {
  const patch: JsonPatchDocument = [];

  // Get all unique keys from both objects
  const allKeys = new Set([...Object.keys(original), ...Object.keys(updated)]);

  for (const key of allKeys) {
    const originalValue = original[key];
    const updatedValue = updated[key];

    // Skip if values are identical (handles primitives and object references)
    if (originalValue === updatedValue) continue;

    // Handle deep equality for objects and arrays
    if (isDeepEqual(originalValue, updatedValue)) continue;

    const path = `/${key}`;

    // Remove operation: key exists in original but not in updated (or explicitly undefined)
    if (key in original && !(key in updated)) {
      patch.push({ op: 'remove', path });
      continue;
    }

    // Add operation: key doesn't exist in original
    if (!(key in original) && key in updated) {
      patch.push({ op: 'add', path, value: updatedValue });
      continue;
    }

    // Replace operation: key exists in both with different values
    patch.push({ op: 'replace', path, value: updatedValue });
  }

  return patch;
}

/**
 * Deep equality check for objects and arrays
 */
function isDeepEqual(value1: unknown, value2: unknown): boolean {
  // Strict equality covers primitives and same object references
  if (value1 === value2) return true;

  // Type mismatch
  if (typeof value1 !== typeof value2) return false;

  // Null checks (typeof null === 'object')
  if (value1 === null || value2 === null) return false;

  // Date comparison
  if (value1 instanceof Date && value2 instanceof Date) {
    return value1.getTime() === value2.getTime();
  }

  // Array comparison
  if (Array.isArray(value1) && Array.isArray(value2)) {
    if (value1.length !== value2.length) return false;
    return value1.every((item, index) => isDeepEqual(item, value2[index]));
  }

  // Object comparison
  if (typeof value1 === 'object' && typeof value2 === 'object') {
    const keys1 = Object.keys(value1 as Record<string, unknown>);
    const keys2 = Object.keys(value2 as Record<string, unknown>);

    if (keys1.length !== keys2.length) return false;

    return keys1.every((key) =>
      isDeepEqual((value1 as Record<string, unknown>)[key], (value2 as Record<string, unknown>)[key])
    );
  }

  return false;
}
