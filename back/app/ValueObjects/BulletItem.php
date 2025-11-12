<?php

namespace App\ValueObjects;

class BulletItem
{
    public function __construct(
        public string $content,
        public int $position = 0
    ) {}

    public function toArray(): array
    {
        return [
            'content' => $this->content,
            'position' => $this->position,
        ];
    }

    public static function fromArray(array $data): self
    {
        return new self(
            content: $data['content'],
            position: $data['position'] ?? 0
        );
    }
}
