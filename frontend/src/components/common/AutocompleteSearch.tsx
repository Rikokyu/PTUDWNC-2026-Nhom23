"use client";

import { useId, useState, type FormEvent, type KeyboardEvent } from "react";
import { Search, X } from "lucide-react";
import { Button } from "@/components/ui/Button";

export interface SearchSuggestion {
  id: string;
  label: string;
  description?: string;
}

interface AutocompleteSearchProps {
  label: string;
  placeholder: string;
  value: string;
  onChange: (value: string) => void;
  onSearch: (value: string) => void;
  onSelect?: (suggestion: SearchSuggestion) => void;
  suggestions: SearchSuggestion[];
  isLoading?: boolean;
}

export function AutocompleteSearch({
  label,
  placeholder,
  value,
  onChange,
  onSearch,
  onSelect,
  suggestions,
  isLoading = false,
}: AutocompleteSearchProps) {
  const id = useId();
  const [isOpen, setIsOpen] = useState(false);
  const [activeIndex, setActiveIndex] = useState(-1);
  const showSuggestions = isOpen && value.trim().length >= 1;

  function submit(valueToSearch: string) {
    const normalized = valueToSearch.trim();
    onChange(normalized);
    onSearch(normalized);
    setIsOpen(false);
    setActiveIndex(-1);
  }

  function selectSuggestion(suggestion: SearchSuggestion) {
    if (!onSelect) {
      submit(suggestion.label);
      return;
    }
    onChange(suggestion.label);
    setIsOpen(false);
    setActiveIndex(-1);
    onSelect(suggestion);
  }

  function handleSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    submit(value);
  }

  function handleKeyDown(event: KeyboardEvent<HTMLInputElement>) {
    if (event.key === "Escape") {
      setIsOpen(false);
      setActiveIndex(-1);
    } else if (showSuggestions && suggestions.length > 0 && event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((index) => (index + 1) % suggestions.length);
    } else if (showSuggestions && suggestions.length > 0 && event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((index) => (index <= 0 ? suggestions.length - 1 : index - 1));
    } else if (showSuggestions && event.key === "Enter" && activeIndex >= 0) {
      event.preventDefault();
      selectSuggestion(suggestions[activeIndex]);
    }
  }

  return (
    <div
      className="relative z-20 w-full"
      onBlur={(event) => {
        if (!event.currentTarget.contains(event.relatedTarget)) {
          setIsOpen(false);
          setActiveIndex(-1);
        }
      }}
    >
      <form
        onSubmit={handleSubmit}
        className="flex w-full items-center gap-2 rounded-2xl border border-stone-300 bg-surface-cream p-1.5 shadow-subtle transition-colors focus-within:border-brand focus-within:ring-2 focus-within:ring-brand/15"
      >
        <label htmlFor={id} className="sr-only">{label}</label>
        <Search className="ml-2 h-5 w-5 shrink-0 text-content-tertiary" aria-hidden="true" />
        <input
          id={id}
          type="search"
          role="combobox"
          aria-autocomplete="list"
          aria-expanded={showSuggestions}
          aria-controls={showSuggestions ? `${id}-suggestions` : undefined}
          aria-activedescendant={showSuggestions && activeIndex >= 0 ? `${id}-option-${activeIndex}` : undefined}
          autoComplete="off"
          value={value}
          maxLength={100}
          onFocus={() => setIsOpen(true)}
          onChange={(event) => {
            onChange(event.target.value);
            setIsOpen(true);
            setActiveIndex(-1);
          }}
          onKeyDown={handleKeyDown}
          placeholder={placeholder}
          className="h-10 min-w-0 flex-1 bg-transparent px-1 text-sm text-content-primary outline-none placeholder:text-content-tertiary focus:outline-none"
        />
        {value && (
          <button
            type="button"
            aria-label="Xóa từ khóa"
            onClick={() => submit("")}
            className="rounded-full p-1.5 text-content-tertiary hover:bg-zinc-200 hover:text-content-primary"
          >
            <X className="h-4 w-4" aria-hidden="true" />
          </button>
        )}
        <Button type="submit" size="md" className="shrink-0">Tìm kiếm</Button>
      </form>
      {showSuggestions && (
        <div
          id={`${id}-suggestions`}
          role="listbox"
          aria-label={`Gợi ý ${label.toLowerCase()}`}
          className="absolute inset-x-0 top-full z-30 mt-2 max-h-72 overflow-y-auto rounded-2xl border border-stone-200 bg-surface p-1.5 shadow-dropdown"
        >
          {isLoading && suggestions.length === 0 ? (
            <p className="px-3 py-2.5 text-sm text-content-secondary">Đang tìm gợi ý…</p>
          ) : suggestions.length === 0 ? (
            <p className="px-3 py-2.5 text-sm text-content-secondary">Không có gợi ý phù hợp.</p>
          ) : suggestions.map((suggestion, index) => (
            <button
              key={suggestion.id}
              id={`${id}-option-${index}`}
              type="button"
              role="option"
              aria-selected={activeIndex === index}
              onMouseDown={(event) => event.preventDefault()}
              onClick={() => selectSuggestion(suggestion)}
              className={`flex w-full flex-col rounded-lg px-3 py-2 text-left transition-colors hover:bg-brand-light ${activeIndex === index ? "bg-brand-light" : ""}`}
            >
              <span className="text-sm font-semibold text-content-primary">{suggestion.label}</span>
              {suggestion.description && (
                <span className="line-clamp-1 text-xs text-content-secondary">{suggestion.description}</span>
              )}
            </button>
          ))}
        </div>
      )}
    </div>
  );
}
