export type Pagination = {
  // Keys has to match the keys defined in the PaninationMetadata in the API
  currentPage: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type PaginatedResult<T> = {
  items: T[];
  metadata: Pagination; // The above export type
};
