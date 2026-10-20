import { Button } from "@/components/ui/button";

type PaginationProps = {
  currentPage: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

export function Pagination({
  currentPage,
  totalPages,
  onPageChange,
}: PaginationProps) {
  if (totalPages < 1) return null;

  return (
    <nav className="pagination" aria-label="Pagination">
      <Button
        type="button"
        variant="outline"
        disabled={currentPage <= 1}
        onClick={() => onPageChange(Math.max(1, currentPage - 1))}
      >
        Trước
      </Button>
      <span aria-live="polite">
        Trang {currentPage} / {totalPages}
      </span>
      <Button
        type="button"
        variant="outline"
        disabled={currentPage >= totalPages}
        onClick={() => onPageChange(Math.min(totalPages, currentPage + 1))}
      >
        Sau
      </Button>
    </nav>
  );
}
