import type { Editor, FileLoader } from 'ckeditor5';
import { FileRepository } from 'ckeditor5';
import { Subscription } from 'rxjs';
import { UploadService } from '../services/upload.service';

// Thay thế cho CKFinder (thương mại, không miễn phí) — upload ảnh chèn trực tiếp trong
// CKEditor bằng chính api/uploads/image sẵn có của app, không cần license/connector riêng.
class ImageUploadAdapter {
  private subscription: Subscription | null = null;

  constructor(private loader: FileLoader, private uploadService: UploadService) {}

  upload(): Promise<{ default: string }> {
    return this.loader.file.then(
      (file) =>
        new Promise<{ default: string }>((resolve, reject) => {
          if (!file) {
            reject('Không có file để tải lên');
            return;
          }
          this.subscription = this.uploadService.uploadImage(file as File).subscribe({
            next: (res) => {
              if (res.success && res.data) {
                resolve({ default: res.data.url });
              } else {
                reject(res.message || 'Tải ảnh lên thất bại');
              }
            },
            error: (err) => reject(err?.error?.message || 'Tải ảnh lên thất bại')
          });
        })
    );
  }

  abort(): void {
    this.subscription?.unsubscribe();
  }
}

export function createImageUploadAdapterPlugin(uploadService: UploadService): (editor: Editor) => void {
  return function ImageUploadAdapterPlugin(editor: Editor): void {
    editor.plugins.get(FileRepository).createUploadAdapter = (loader: FileLoader) =>
      new ImageUploadAdapter(loader, uploadService);
  };
}
