<?php

namespace App\ValueObjects;

class Entry
{
    public function __construct(
        public string $title,
        public ?string $start_date = null,
        public ?string $end_date = null,
        public int $position = 0
    ) {}

    public function toArray(): array
    {
        return [
            'title' => $this->title,
            'start_date' => $this->start_date,
            'end_date' => $this->end_date,
            'position' => $this->position,
        ];
    }

    public static function fromArray(array $data): self
    {
        return new self(
            title: $data['title'],
            start_date: $data['start_date'] ?? null,
            end_date: $data['end_date'] ?? null,
            position: $data['position'] ?? 0
        );
    }
}
