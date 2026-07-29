/** Backend/DTOs/DocumentDto.cs — StudentDocumentDto, dùng cho trang "Tài liệu chung". */

export interface StudentDocumentApi {
  id: number;
  title: string;
  content?: string;
  fileUrl?: string;
}
