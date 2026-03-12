import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { Book, BookService, BookUpsert } from './book.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  books: Book[] = [];
  loading = false;
  error: string | null = null;
  editing: Book | null = null;
  saving = false;
  private refreshSeq = 0;
  notice: { type: 'success' | 'info' | 'error'; message: string } | null = null;
  private noticeTimeout: any = null;
  query = '';
  sortKey: keyof Book = 'id';
  sortDir: 'asc' | 'desc' = 'asc';

  readonly form = this.fb.nonNullable.group({
    title: ['', Validators.required],
    author: ['', Validators.required],
    isbn: ['', Validators.required],
    publicationDate: ['', Validators.required]
  });

  constructor(
    private readonly fb: FormBuilder,
    private readonly booksApi: BookService
  ) {}

  ngOnInit(): void {
    this.refresh();
    this.startCreate();
  }

  refresh(): void {
    const seq = ++this.refreshSeq;
    this.loading = true;
    this.error = null;

    this.booksApi.getAll().subscribe({
      next: (books) => {
        if (seq !== this.refreshSeq) return;
        this.books = books;
        this.loading = false;
      },
      error: (err) => {
        if (seq !== this.refreshSeq) return;
        this.error = this.toMessage(err);
        this.loading = false;
        this.showNotice(this.error, 'error');
      }
    });
  }

  startCreate(): void {
    this.editing = null;
    this.form.reset({
      title: '',
      author: '',
      isbn: '',
      publicationDate: this.todayIso()
    });
  }

  startEdit(book: Book): void {
    this.editing = book;
    this.form.setValue({
      title: book.title ?? '',
      author: book.author ?? '',
      isbn: book.isbn ?? '',
      publicationDate: (book.publicationDate ?? '').toString().slice(0, 10)
    });
  }

  cancelEdit(): void {
    this.startCreate();
  }

  submit(): void {
    this.error = null;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      this.showNotice('Please fill all fields before submitting.', 'error');
      return;
    }

    const isEdit = !!this.editing;
    const payload: BookUpsert = {
      title: (this.form.controls.title.value ?? '').trim(),
      author: (this.form.controls.author.value ?? '').trim(),
      isbn: (this.form.controls.isbn.value ?? '').trim(),
      publicationDate: (this.form.controls.publicationDate.value ?? '').trim()
    };

    const request$ = this.editing
      ? this.booksApi.update(this.editing.id, payload)
      : this.booksApi.create(payload);

    this.saving = true;
    request$.subscribe({
      next: () => {
        this.startCreate();
        this.saving = false;
        this.showNotice(isEdit ? 'Book updated successfully.' : 'Book created successfully.', 'success');
        this.refresh();
      },
      error: (err) => {
        this.error = this.toMessage(err);
        this.saving = false;
        this.showNotice(this.error, 'error');
      }
    });
  }

  remove(book: Book): void {
    this.error = null;

    if (!confirm(`Delete "${book.title}"?`)) return;

    this.booksApi.delete(book.id).subscribe({
      next: () => {
        this.showNotice('Book deleted successfully.', 'success');
        this.refresh();
      },
      error: (err) => {
        this.error = this.toMessage(err);
        this.showNotice(this.error, 'error');
      }
    });
  }

  trackById(_: number, book: Book): number {
    return book.id;
  }

  get visibleBooks(): Book[] {
    const q = this.query.trim().toLowerCase();
    const filtered =
      q.length === 0
        ? this.books
        : this.books.filter((b) => {
            const haystack = `${b.id} ${b.title} ${b.author} ${b.isbn} ${b.publicationDate}`
              .toLowerCase()
              .trim();
            return haystack.includes(q);
          });

    const dir = this.sortDir === 'asc' ? 1 : -1;
    const key = this.sortKey;

    return [...filtered].sort((a, b) => {
      const av = a[key];
      const bv = b[key];

      if (key === 'id') return (Number(av) - Number(bv)) * dir;

      const as = (av ?? '').toString();
      const bs = (bv ?? '').toString();
      return as.localeCompare(bs) * dir;
    });
  }

  setSort(key: keyof Book): void {
    if (this.sortKey === key) {
      this.sortDir = this.sortDir === 'asc' ? 'desc' : 'asc';
      return;
    }
    this.sortKey = key;
    this.sortDir = 'asc';
  }

  sortLabel(key: keyof Book): string {
    if (this.sortKey !== key) return '';
    return this.sortDir === 'asc' ? '▲' : '▼';
  }

  private toMessage(err: unknown): string {
    try {
      const anyErr = err as any;
      const validationErrors = anyErr?.error?.errors;
      if (validationErrors && typeof validationErrors === 'object') {
        const firstKey = Object.keys(validationErrors)[0];
        const firstMsg = firstKey ? validationErrors[firstKey]?.[0] : null;
        if (typeof firstMsg === 'string' && firstMsg.trim()) return firstMsg;
      }

      return (
        anyErr?.error?.title ||
        anyErr?.error?.message ||
        anyErr?.message ||
        'Request failed'
      );
    } catch {
      return 'Request failed';
    }
  }

  private showNotice(message: string, type: 'success' | 'info' | 'error', ms = 2500): void {
    if (!message) return;
    this.notice = { type, message };
    if (this.noticeTimeout) clearTimeout(this.noticeTimeout);
    this.noticeTimeout = setTimeout(() => {
      this.notice = null;
      this.noticeTimeout = null;
    }, ms);
  }

  private todayIso(): string {
    const now = new Date();
    const y = now.getFullYear();
    const m = String(now.getMonth() + 1).padStart(2, '0');
    const d = String(now.getDate()).padStart(2, '0');
    return `${y}-${m}-${d}`;
  }
}
