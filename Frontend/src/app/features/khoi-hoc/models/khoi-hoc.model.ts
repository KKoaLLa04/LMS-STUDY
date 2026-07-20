export interface KhoiHoc {
  id: number;
  name: string;
  code: string;
  orderNumber: number;
}

export interface CreateKhoiHocRequest {
  name: string;
  code: string;
  orderNumber: number;
}

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data?: T;
  httpStatusCode?: number;
}
